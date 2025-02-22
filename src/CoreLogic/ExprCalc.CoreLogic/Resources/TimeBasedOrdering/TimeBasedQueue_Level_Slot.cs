using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.CoreLogic.Resources.TimeBasedOrdering
{
    internal partial class TimeBasedQueue<T>
    {
        /// <summary>
        /// Full resolution of timer. We use `ulong` to represent time, thus it is equal to number of bits in `ulong`
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
        private const int LevelsCount = 1 + FullResolutionInBits / SlotBitsLength;

        /// <summary>
        /// Represents a single slot, which contains the head index of linked list of all items within this slot
        /// </summary>
        private struct TimeSlot
        {
            public int ListHead;
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
            public TimeSlotsArray Slots;

            public void Reset()
            {
                for (int i = 0; i < LevelSize; i++)
                    Slots[i].ListHead = LinkedLists.NoNextItem;
            }
        }

        /// <summary>
        /// Calculates level and slot indexes for new item
        /// </summary>
        /// <param name="now">Current time</param>
        /// <param name="expiry">Expiry time of a new item</param>
        /// <returns>Indexes</returns>
        private static (int level, int slot) GetLevelAndSlotIndexes(ulong now, ulong expiry)
        {
            Debug.Assert(now <= expiry);

            // Take highest bits that differ between `now` and `expiry` and then calculate the level offset
            int level = (FullResolutionInBits - BitOperations.LeadingZeroCount((now ^ expiry) | 1ul) - 1) / SlotBitsLength;
            Debug.Assert(level >= 0 && level < LevelsCount);

            // Get slot index within level and cut it to the beggining of the slot
            ulong now_slot = (now >> (level * SlotBitsLength)) & (ulong.MaxValue << SlotBitsLength);
            // Do the same for `now` and calculate offset
            ulong slot = (expiry >> (level * SlotBitsLength)) - now_slot;

            Debug.Assert(slot >= 0 && slot < LevelSize);

            return (level, (int)slot);
        }
    }
}
