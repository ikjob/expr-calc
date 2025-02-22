using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.CoreLogic.Resources.TimeBasedOrdering
{
    internal partial class TimeBasedQueue<T>
    {
        private LinkedLists _linkedLists;
        private int _availableItemsListHead;
        private int _availableItemsListTail;
        private int _availableItemsCount;

        private ulong _now;
        private readonly TimeLevel[] _levels;

        public TimeBasedQueue(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity));

            _linkedLists = new LinkedLists(capacity);
            _levels = new TimeLevel[LevelsCount];
            _now = 0;

            _availableItemsListHead = LinkedLists.NoNextItem;
            _availableItemsListTail = LinkedLists.NoNextItem;
            _availableItemsCount = 0;

            for (int i = 0; i < _levels.Length; i++)
                _levels[i].Reset();
        }

        public int Count => _linkedLists.Count;
        public int AvailableCount => _availableItemsCount;
        public int Capacity => _linkedLists.Capacity;

        private void AddToAvailablesList(T item)
        {
            _availableItemsListTail = _linkedLists.AddToListTail(item, _availableItemsListTail);
            if (_availableItemsListHead == LinkedLists.NoNextItem)
                _availableItemsListHead = _availableItemsListTail;

            _availableItemsCount++;
        }

        public void Add(T item, ulong availableAfter, out int availableCountDelta)
        {
            if (availableAfter <= _now)
            {
                AddToAvailablesList(item);
                availableCountDelta = 1;
                return;
            }

            (int levelIndex, int slotIndex) = GetLevelAndSlotIndexes(_now, availableAfter);
            ref TimeSlot slot = ref _levels[levelIndex].Slots[slotIndex];
            slot.ListHead = _linkedLists.AddToListHead(item, slot.ListHead);
            availableCountDelta = 0;
        }


        public bool TryTake([MaybeNullWhen(false)] out T item)
        {
            if (_availableItemsListHead == LinkedLists.NoNextItem)
            {
                item = default;
                return false;
            }

            int newHead = _linkedLists.RemoveFromListHead(_availableItemsListHead, out item);
            Debug.Assert(newHead != LinkedLists.NoNextItem || (_availableItemsListHead == _availableItemsListTail));
            _availableItemsListHead = newHead;
            if (newHead == LinkedLists.NoNextItem)
                _availableItemsListTail = LinkedLists.NoNextItem;

            _availableItemsCount--;
            return true;
        }



        public int AdvanceTime(ulong now)
        {
            if (now < _now)
                throw new ArgumentException("Only forward advance possible");

            for (int level = 0; level < _levels.Length; level++)
            {

            }


            return 0;
        }
    }
}
