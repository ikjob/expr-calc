using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using LinkedListIndex = int;

namespace ExprCalc.CoreLogic.Resources.TimeBasedOrdering
{
    internal partial class TimeBasedQueue<T>
    {
        /// <summary>
        /// Full resolution of timer.
        /// `ulong` is used to represent time, thus the resolution is equal to number of bits in `ulong`
        /// </summary>
        private const int FullResolutionInBits = 64;
        /// <summary>
        /// Amount of bits used to represent a slot index
        /// </summary>
        private const int SlotBitsLength = 6;
        /// <summary>
        /// Number of items in every level
        /// </summary>
        private const int LevelSize = 1 << SlotBitsLength;
        /// <summary>
        /// Number of levels required to cover the full resolution of timer
        /// </summary>
        private const int LevelsCount = (FullResolutionInBits / SlotBitsLength) + 1;

        /// <summary>
        /// Represents a single slot, which contains the head index of linked list of all items within this slot
        /// </summary>
        private struct TimeSlot
        {
            public LinkedListIndex ListHead;

            public readonly bool IsEmpty => ListHead == LinkedLists.NoNextItem;
        }


        /// <summary>
        /// Inlined array of <see cref="TimeSlot"/>
        /// </summary>
        /// <remarks>
        /// <see cref="InlineArrayAttribute"/> allows to reduce the amount of required allocations
        /// </remarks>
        [InlineArray(LevelSize)]
        private struct TimeSlotsArray
        {
            private TimeSlot _slot0;
        }

        /// <summary>
        /// Represents time level. Contains an array of slots of that level
        /// </summary>
        private struct TimeLevel
        {
            private TimeSlotsArray _slots;
            /// <summary>
            /// Bitmap that contains '1' at offset equal to index in <see cref="_slots"/>, when that slot is not empty
            /// </summary>
            private ulong _occupiedBitmap;

            public readonly bool IsEmpty
            {
                get
                {
                    Debug.Assert(SlotBitsLength <= 6, "Bitmap optimisation works only when number of slots per level is <= 64");
                    return _occupiedBitmap == 0;
                }
            }

            public readonly int FirstNonEmptySlot
            {
                get
                {
                    Debug.Assert(SlotBitsLength <= 6, "Bitmap optimisation works only when number of slots per level is <= 64");
                    return BitOperations.TrailingZeroCount(_occupiedBitmap);
                }
            }

            public void Reset()
            {
                for (int i = 0; i < LevelSize; i++)
                    _slots[i].ListHead = LinkedLists.NoNextItem;
                _occupiedBitmap = 0;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly bool IsSlotEmpty(int slotIndex)
            {
                return _slots[slotIndex].IsEmpty;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly LinkedListIndex GetSlotListHead(int slotIndex)
            {
                return _slots[slotIndex].ListHead;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SetSlotListHead(int slotIndex, LinkedListIndex newListHead)
            {
                Debug.Assert(newListHead != LinkedLists.NoNextItem);

                _slots[slotIndex].ListHead = newListHead;
                _occupiedBitmap = _occupiedBitmap | (1ul << slotIndex);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public LinkedListIndex ResetSlotListHead(int slotIndex)
            {
                int result = _slots[slotIndex].ListHead;
                _slots[slotIndex].ListHead = LinkedLists.NoNextItem;
                _occupiedBitmap = _occupiedBitmap & ~(1ul << slotIndex);
                return result;
            }
        }

        /// <summary>
        /// Calculates level and slot indexes for new item
        /// </summary>
        /// <param name="now">Current time</param>
        /// <param name="itemTimepoint">Timepoint of a new item</param>
        /// <returns>Indexes</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static (int levelIndex, int slotIndex) GetLevelAndSlotIndexes(ulong now, ulong itemTimepoint)
        {
            Debug.Assert(now <= itemTimepoint);

            // Take highest bits that differ between `now` and `expiry` and then calculate the level offset
            int level = (FullResolutionInBits - BitOperations.LeadingZeroCount((now ^ itemTimepoint) | 1ul) - 1) / SlotBitsLength;
            Debug.Assert(level >= 0 && level < LevelsCount);

            // Get slot index within level and cut it to the beggining of the slot
            ulong now_slot = (now >> (level * SlotBitsLength)) & (ulong.MaxValue << SlotBitsLength);
            // Do the same for `now` and calculate offset
            ulong slot = (itemTimepoint >> (level * SlotBitsLength)) - now_slot;

            Debug.Assert(slot >= 0 && slot < LevelSize);

            return (level, (int)slot);
        }

        /// <summary>
        /// Calculates the first and last timepoint that lies within the slot
        /// </summary>
        /// <param name="now">Current time</param>
        /// <param name="levelIndex">Index of level</param>
        /// <param name="slotIndex">Index of slot</param>
        /// <returns>Timepoints</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static (ulong start, ulong end) GetSlotStartEndTimepoint(ulong now, int levelIndex, int slotIndex)
        {
            Debug.Assert(levelIndex < LevelsCount);
            Debug.Assert(slotIndex < LevelSize);

            int shift = levelIndex * SlotBitsLength;
            // Double shift is to avoid overflow when levelIndex == 10
            ulong start = (now & ((ulong.MaxValue << shift) << SlotBitsLength)) | ((ulong)slotIndex << shift);
            ulong end = unchecked(start + (1ul << shift) - 1ul);

            return (start, end);
        }

        /// <summary>
        /// Returns slot index at level <paramref name="levelIndex"/> for <paramref name="timepoint"/>
        /// </summary>
        /// <param name="timepoint">Timepoint</param>
        /// <param name="levelIndex">Level index</param>
        /// <returns>Calculated slot index</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetSlotIndexOnLevelForTimepoint(ulong timepoint, int levelIndex)
        {
            Debug.Assert(levelIndex < LevelsCount);

            int shift = levelIndex * SlotBitsLength;
            return (int)((timepoint >> shift) & ((ulong)LevelSize - 1ul));
        }
    }
}
