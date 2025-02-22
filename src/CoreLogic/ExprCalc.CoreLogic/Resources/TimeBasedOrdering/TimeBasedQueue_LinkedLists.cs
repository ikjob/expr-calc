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
            public ulong Timepoint;
            public LinkedListIndex Next;
        }

        private struct LinkedListHeadTail(LinkedListIndex head, LinkedListIndex tail)
        {
            public LinkedListIndex Head = head;
            public LinkedListIndex Tail = tail;

            public readonly bool IsEmpty => Head == LinkedLists.NoNextItem;

            public static LinkedListHeadTail Empty()
            {
                return new LinkedListHeadTail(LinkedLists.NoNextItem, LinkedLists.NoNextItem);
            }
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


            /// <summary>
            /// Traverse list from <paramref name="headIndex"/> up to its tail.
            /// Returns head and tail pair inside <see cref="LinkedListHeadTail"/> and items count in that list.
            /// </summary>
            public readonly LinkedListHeadTail BuildLinkedListHeadTail(LinkedListIndex headIndex, out int count)
            {
                int countLocal = 0;
                int tailIndex = NoNextItem;
                int currentIndex = headIndex;
                while (currentIndex != NoNextItem)
                {
                    countLocal++;
                    (tailIndex, currentIndex) = (currentIndex, _items[currentIndex].Next);
                }

                count = countLocal;
                return new LinkedListHeadTail(headIndex, tailIndex);
            }


            /// <summary>
            /// Adds new item to the head of the list. Previous head is passed in <paramref name="listHead"/>.
            /// Returns new item index which is also a new list head.
            /// </summary>
            public LinkedListIndex AddToListHead(T item, ulong timepoint, LinkedListIndex listHead)
            {
                if (_count == _items.Length)
                    Grow();

                int newItemIndex = _nextFreeSlot;
                bool nextFreeSlotUsed = true;
                if (newItemIndex == NoNextItem)
                {
                    newItemIndex = _count;
                    nextFreeSlotUsed = false;
                }

                ref LinkedListItem result = ref _items[newItemIndex];
                if (nextFreeSlotUsed)
                    _nextFreeSlot = result.Next;

                result.Item = item;
                result.Timepoint = timepoint;
                result.Next = listHead;
                _count++;
                return newItemIndex;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public LinkedListIndex AddToNewList(T item, ulong timepoint)
            {
                return AddToListHead(item, timepoint, NoNextItem);
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public LinkedListIndex AddToListTail(T item, ulong timepoint, LinkedListIndex listTail)
            {
                int result = AddToListHead(item, timepoint, NoNextItem);
                if (listTail != NoNextItem)
                {
                    Debug.Assert(_items[listTail].Next == NoNextItem);
                    _items[listTail].Next = result;
                }
                return result;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void AddToListTail(ref LinkedListHeadTail list, T item, ulong timepoint)
            {
                int newItemIndex = AddToListHead(item, timepoint, NoNextItem);
                if (list.Tail == NoNextItem)
                {
                    Debug.Assert(list.Head == NoNextItem);
                    list.Tail = newItemIndex;
                    list.Head = newItemIndex;
                }
                else
                {
                    Debug.Assert(list.Head != NoNextItem);
                    Debug.Assert(_items[list.Tail].Next == NoNextItem);
                    _items[list.Tail].Next = newItemIndex;
                    list.Tail = newItemIndex;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly void LinkListHeadToListTail(ref LinkedListHeadTail listA, in LinkedListHeadTail listB)
            {
                if (listA.Tail == NoNextItem)
                {
                    Debug.Assert(listA.Head == NoNextItem);

                    listA.Head = listB.Head;
                    listA.Tail = listB.Tail;
                }
                else
                {
                    Debug.Assert(listA.Head != NoNextItem);
                    Debug.Assert(_items[listA.Tail].Next == NoNextItem);

                    if (listB.Head != NoNextItem)
                    {
                        Debug.Assert(listB.Tail != NoNextItem);
                        Debug.Assert(_items[listB.Tail].Next == NoNextItem);

                        _items[listA.Tail].Next = listB.Head;
                        listA.Tail = listB.Tail;
                    }
                    else
                    {
                        Debug.Assert(listB.Tail == NoNextItem);
                    }
                }
            }


            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly LinkedListIndex RelinkToListHead(LinkedListIndex itemIndex, LinkedListIndex listHead)
            {
                Debug.Assert(itemIndex != NoNextItem);
                _items[itemIndex].Next = listHead;
                return itemIndex;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly void RelinkToListTail(LinkedListIndex itemIndex, ref LinkedListHeadTail list)
            {
                Debug.Assert(itemIndex != NoNextItem);
                _items[itemIndex].Next = NoNextItem;

                if (list.Tail == NoNextItem)
                {
                    Debug.Assert(list.Head == NoNextItem);
                    list.Head = itemIndex;
                    list.Tail = itemIndex;
                }
                else
                {
                    Debug.Assert(list.Head != NoNextItem);
                    _items[list.Tail].Next = itemIndex;
                    list.Tail = itemIndex;
                }
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
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void RemoveFromListHead(ref LinkedListHeadTail list, out T item)
            {
                Debug.Assert(list.Head != NoNextItem);
                Debug.Assert(list.Tail != NoNextItem);

                int newHead = RemoveFromListHead(list.Head, out item);
                Debug.Assert(newHead != NoNextItem || (list.Tail == list.Head));
                list.Head = newHead;
                if (newHead == NoNextItem)
                    list.Tail = NoNextItem;
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
