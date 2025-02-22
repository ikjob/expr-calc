using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using LinkedListIndex = int;

namespace ExprCalc.CoreLogic.Resources.TimeBasedOrdering
{
    internal partial class TimeBasedQueue<T>
    {
        private struct LinkedListItem
        {
            public T Item;
            public LinkedListIndex Next;
        }

        /// <summary>
        /// Efficient storage for multiple lined lists.
        /// Links are based on the indexes within array
        /// </summary>
        private struct LinkedLists
        {
            public const LinkedListIndex NoNextItem = -1;

            private LinkedListItem[] _items;
            private int _count;
            /// <summary>
            /// Next free slot index
            /// </summary>
            /// <remarks>
            /// If it is equal to <see cref="NoNextItem"/> and 
            /// <see cref="_count"/> is less than <see cref="_items"/> Length, 
            /// then there are still empty slots starting from <see cref="_count"/>.
            /// This is an optimization to avoid reindexing on Grow
            /// </remarks>
            private LinkedListIndex _nextFreeSlot;
            
            public LinkedLists(int capacity)
            {
                _items = capacity == 0 ? Array.Empty<LinkedListItem>() : new LinkedListItem[capacity];
                _count = 0;
                _nextFreeSlot = NoNextItem;           
            }

            public readonly int Count => _count;
            public readonly int Capacity => _items.Length;
            public readonly ref LinkedListItem this[LinkedListIndex index] => ref _items[index];

            public LinkedListIndex AddToListHead(T item, LinkedListIndex currentHead)
            {
                if (_count == _items.Length)
                    Grow();

                int index = _nextFreeSlot;
                bool nextFreeSlotUsed = true;
                if (index == NoNextItem)
                {
                    index = _count;
                    nextFreeSlotUsed = false;
                }

                ref LinkedListItem result = ref _items[index];
                if (nextFreeSlotUsed)
                    _nextFreeSlot = result.Next;

                result.Item = item;
                result.Next = currentHead;
                _count++;
                return index;
            }
            public LinkedListIndex AddToNewList(T item)
            {
                return AddToListHead(item, NoNextItem);
            }
            public LinkedListIndex AddToListTail(T item, LinkedListIndex currentTail)
            {
                int result = AddToListHead(item, NoNextItem);
                if (currentTail != NoNextItem)
                    _items[currentTail].Next = result;
                return result;
            }

            public LinkedListIndex RemoveFromListHead(LinkedListIndex index, out T item)
            {
                ref LinkedListItem slot = ref _items[index];
                int result = slot.Next;
                item = slot.Item;

                slot.Next = _nextFreeSlot;
                _nextFreeSlot = index;
                _count--;

                if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
                {
                    slot.Item = default!;
                }
                
                return result;
            }

            private void Grow()
            {
                Debug.Assert(_count == _items.Length);
                Debug.Assert(_nextFreeSlot == NoNextItem);

                int newSize = _items.Length == 0 ? 4 : unchecked(_items.Length * 2);
                if (newSize < 0)
                    newSize = int.MaxValue;

                Array.Resize(ref _items, newSize);
            }
        }
    }
}
