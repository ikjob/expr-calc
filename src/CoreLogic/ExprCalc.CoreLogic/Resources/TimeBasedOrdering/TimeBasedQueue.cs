using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinkedListIndex = int;

namespace ExprCalc.CoreLogic.Resources.TimeBasedOrdering
{
    /// <summary>
    /// Time based queue returns items only when their storage time expired
    /// </summary>
    /// <remarks>
    /// Hierarchical timing wheel approach is implemented.
    /// General idea described here (scheme 7): https://paulcavallaro.com/blog/hashed-and-hierarchical-timing-wheels/
    /// </remarks>
    internal partial class TimeBasedQueue<T>
    {
        private LinkedLists _linkedLists;
        private LinkedListHeadTail _availableItemsList;
        private int _availableItemsCount;

        private ulong _currentTimepoint;
        private readonly TimeLevel[] _levels;

        public TimeBasedQueue(ulong currentTimepoint, int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity));

            _linkedLists = new LinkedLists(capacity);
            _levels = new TimeLevel[LevelsCount];
            _currentTimepoint = currentTimepoint;

            _availableItemsList = LinkedListHeadTail.Empty();
            _availableItemsCount = 0;

            for (int i = 0; i < _levels.Length; i++)
                _levels[i].Reset();
        }

        public int Count => _linkedLists.Count;
        public int AvailableCount => _availableItemsCount;
        public int Capacity => _linkedLists.Capacity;
        public ulong CurrentTimepoint => _currentTimepoint;


        /// <summary>
        /// Adds new item to the queue
        /// </summary>
        /// <param name="item">Item</param>
        /// <param name="availableAfter">Availability timepoint</param>
        /// <param name="availableCountDelta">Set 1 if new item became available immediately, otherwise 0</param>
        public void Add(T item, ulong availableAfter, out int availableCountDelta)
        {
            if (availableAfter <= _currentTimepoint)
            {
                _linkedLists.AddToListTail(ref _availableItemsList, item, availableAfter);
                _availableItemsCount++;
                availableCountDelta = 1;
                return;
            }

            (int levelIndex, int slotIndex) = GetLevelAndSlotIndexes(_currentTimepoint, availableAfter);
            ref TimeLevel level = ref _levels[levelIndex];
            level.SetSlotListHead(slotIndex, _linkedLists.AddToListHead(item, availableAfter, level.GetSlotListHead(slotIndex)));
            availableCountDelta = 0;
        }

        /// <summary>
        /// Atttempts to take available item
        /// </summary>
        /// <param name="item">Taken item</param>
        /// <returns>Success</returns>
        public bool TryTake([MaybeNullWhen(false)] out T item)
        {
            if (_availableItemsList.IsEmpty)
            {
                item = default;
                return false;
            }
            else
            {
                _linkedLists.RemoveFromListHead(ref _availableItemsList, out item);
                _availableItemsCount--;
                return true;
            }
        }


        /// <summary>
        /// Rebuild slot list by moving its items to the new slots according to <paramref name="newTimepoint"/>.
        /// Some items goes into available items list.
        /// </summary>
        /// <param name="newTimepoint">New timepoint</param>
        /// <param name="slotListHead">List head index associated with slot</param>
        /// <returns>Number of items that became available after this operation</returns>
        private int RebuildTimeSlotForNewTime(ulong newTimepoint, LinkedListIndex slotListHead)
        {
            int movedToAvailable = 0;
            LinkedListIndex currentIndex = slotListHead;

            while (currentIndex != LinkedLists.NoNextItem)
            {
                ref LinkedListItem item = ref _linkedLists[currentIndex];
                LinkedListIndex nextIndex = item.Next;

                if (item.Timepoint <= newTimepoint)
                {
                    _linkedLists.RelinkToListWithOrdering(currentIndex, ref _availableItemsList);
                    movedToAvailable++;
                }
                else
                {
                    (int levelIndex, int slotIndex) = GetLevelAndSlotIndexes(newTimepoint, item.Timepoint);
                    ref TimeLevel newLevel = ref _levels[levelIndex];
                    newLevel.SetSlotListHead(slotIndex, _linkedLists.RelinkToListHead(currentIndex, newLevel.GetSlotListHead(slotIndex)));
                }

                currentIndex = nextIndex;
            }


            _availableItemsCount += movedToAvailable;
            return movedToAvailable;
        }

        /// <summary>
        /// Advances current timepoint to new position, rebuilds slot structure, moves expired items to availables list
        /// </summary>
        /// <param name="newTimepoint">New timepoint</param>
        /// <returns>Number of items that became available</returns>
        public int AdvanceTime(ulong newTimepoint)
        {
            if (newTimepoint < _currentTimepoint)
                throw new ArgumentException("Only forward advance possible");

            int availableDelta = 0;
            for (int levelIndex = 0; levelIndex < _levels.Length; levelIndex++)
            {
                ref TimeLevel level = ref _levels[levelIndex];
                for (int slotIndex = level.FirstNonEmptySlot; slotIndex < LevelSize; slotIndex++)
                {
                    if (level.IsSlotEmpty(slotIndex))
                        continue;

                    ulong slotStartTimepoint = GetSlotStartTimepoint(_currentTimepoint, levelIndex, slotIndex);
                    if (slotStartTimepoint > newTimepoint)
                    {
                        _currentTimepoint = newTimepoint;
                        return availableDelta;
                    }

                    // Advance time
                    _currentTimepoint = newTimepoint;
                    LinkedListIndex slotListHead = level.ResetSlotListHead(slotIndex);
                    availableDelta += RebuildTimeSlotForNewTime(newTimepoint, slotListHead);
                }
            }


            _currentTimepoint = newTimepoint;
            return availableDelta;
        }

        /// <summary>
        /// Returns closest timepoint on which some item can become available.
        /// Returns null if there are no scheduled items
        /// </summary>
        /// <returns>Closes timepoint</returns>
        public ulong? ClosestTimepoint()
        {
            for (int levelIndex = 0; levelIndex < _levels.Length; levelIndex++)
            {
                int startSlotIndex = GetSlotIndexOnLevelForTimepoint(_currentTimepoint, levelIndex);
                startSlotIndex = Math.Max(startSlotIndex, _levels[levelIndex].FirstNonEmptySlot);
                if (startSlotIndex < LevelSize)
                    return GetSlotStartTimepoint(_currentTimepoint, levelIndex, startSlotIndex);
            }

            return null;
        }


#if INCLUDE_TEST_HELPERS
        private class TimeBasedQueueValidationException(string message) : Exception(message) { }
#endif

        /// <summary>
        /// Correctness validation for tests
        /// </summary>
        internal void ValidateInternalCollectionCorrectness()
        {
#if INCLUDE_TEST_HELPERS
            var heads = _linkedLists.ValidateCorrectness();

            for (int levelIndex = 0; levelIndex < _levels.Length; levelIndex++)
            {
                for (int slotIndex = 0; slotIndex < LevelSize; slotIndex++)
                {
                    var headIndex = _levels[levelIndex].GetSlotListHead(slotIndex);
                    if (headIndex == LinkedLists.NoNextItem && !_levels[levelIndex].IsSlotEmpty(slotIndex))
                        throw new TimeBasedQueueValidationException("Slot emptiness check inconsistency");
                    if (headIndex != LinkedLists.NoNextItem && _levels[levelIndex].IsSlotEmpty(slotIndex))
                        throw new TimeBasedQueueValidationException("Slot emptiness check inconsistency");

                    if (headIndex != LinkedLists.NoNextItem && !heads.Contains(headIndex))
                        throw new TimeBasedQueueValidationException("TimeSlot head references non-existed list");
                }
            }
#endif
        }
    }
}
