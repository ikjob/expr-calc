using ExprCalc.CoreLogic.Resources.TimeBasedOrdering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.CoreLogic.Tests.TimeBasedOrdering
{
    public class TimeBasedQueueTests
    {
        [Fact]
        public void AddImmediatelyAvailableTest()
        {
            ulong startTime = (ulong)Environment.TickCount64;

            var queue = new TimeBasedQueue<string>(startTime, 128);
            Assert.Equal(0, queue.Count);
            Assert.Equal(0, queue.AvailableCount);

            int becameAvail = queue.AdvanceTime(startTime);
            Assert.Equal(0, becameAvail);

            queue.Add("abc", startTime, out int avaiableDelta);
            Assert.Equal(1, avaiableDelta);
            Assert.Equal(1, queue.AvailableCount);

            Assert.True(queue.TryTake(out string? takenItem));
            Assert.Equal("abc", takenItem);

            queue.ValidateInternalCollectionCorrectness();
        }

        [Fact]
        public void AddNotAvailableAndMoveForwardTest()
        {
            ulong startTime = (ulong)Environment.TickCount64;

            var queue = new TimeBasedQueue<string>(startTime, 128);
            Assert.Equal(0, queue.Count);
            Assert.Equal(0, queue.AvailableCount);
            Assert.False(queue.TryTake(out _));

            queue.Add("abc", startTime + 10, out int avaiableDelta);
            Assert.Equal(0, avaiableDelta);
            Assert.Equal(0, queue.AvailableCount);
            Assert.Equal(1, queue.Count);

            queue.ValidateInternalCollectionCorrectness();

            avaiableDelta = queue.AdvanceTime(startTime + 16);
            Assert.Equal(1, avaiableDelta);
            Assert.Equal(1, queue.AvailableCount);
            Assert.Equal(1, queue.Count);

            Assert.True(queue.TryTake(out string? takenItem));
            Assert.Equal("abc", takenItem);
            Assert.Equal(0, queue.AvailableCount);
            Assert.Equal(0, queue.Count);

            queue.ValidateInternalCollectionCorrectness();
        }

        [Fact]
        public void AddItemFarFromNowTest()
        {
            const ulong farAwayOffset = 100000000;
            ulong startTime = (ulong)Environment.TickCount64;

            var queue = new TimeBasedQueue<string>(startTime, 128);
            Assert.Equal(0, queue.Count);
            Assert.Equal(0, queue.AvailableCount);

            queue.Add("abc", startTime + farAwayOffset, out int avaiableDelta);
            Assert.Equal(0, avaiableDelta);
            Assert.Equal(0, queue.AvailableCount);
            Assert.Equal(1, queue.Count);

            for (ulong time = startTime + farAwayOffset / 10; time < startTime + 9 * farAwayOffset / 10; time += farAwayOffset / 10)
            {
                avaiableDelta = queue.AdvanceTime(time);

                Assert.Equal(0, avaiableDelta);
                Assert.Equal(0, queue.AvailableCount);
                Assert.Equal(1, queue.Count);

                queue.ValidateInternalCollectionCorrectness();
            }

            avaiableDelta = queue.AdvanceTime(startTime + farAwayOffset + 10);
            Assert.Equal(1, avaiableDelta);
            Assert.Equal(1, queue.AvailableCount);
            Assert.Equal(1, queue.Count);

            queue.ValidateInternalCollectionCorrectness();

            Assert.True(queue.TryTake(out string? takenItem));
            Assert.Equal("abc", takenItem);
            Assert.Equal(0, queue.AvailableCount);
            Assert.Equal(0, queue.Count);

            queue.ValidateInternalCollectionCorrectness();
        }


        [Fact]
        public void MultipleItemsTest()
        {
            const ulong itemsTimeStep = 1000000;
            ulong startTime = (ulong)Environment.TickCount64;

            var queue = new TimeBasedQueue<string>(startTime, 128);
            Assert.Equal(0, queue.Count);
            Assert.Equal(0, queue.AvailableCount);

            for (int i = 0; i < 1000; i++)
            {
                queue.Add(i.ToString(), startTime + (ulong)(i + 1) * itemsTimeStep, out int avaiableDelta);
                Assert.Equal(0, avaiableDelta);
                Assert.Equal(0, queue.AvailableCount);
                Assert.Equal(i + 1, queue.Count);
            }

            queue.ValidateInternalCollectionCorrectness();

            for (int i = 0; i < 1000; i++)
            {
                queue.AdvanceTime((ulong)i * itemsTimeStep * 2 + startTime);
                if (i % 10 == 0)
                    queue.ValidateInternalCollectionCorrectness();
            }

            Assert.Equal(1000, queue.AvailableCount);
            Assert.Equal(1000, queue.Count);

            for (int i = 0; i < 1000; i++)
            {
                Assert.True(queue.TryTake(out var item));
                Assert.Equal(i.ToString(), item);

                Assert.Equal(1000 - i - 1, queue.AvailableCount);
                Assert.Equal(1000 - i - 1, queue.Count);
            }

            queue.ValidateInternalCollectionCorrectness();
        }


        [Fact]
        public void MultipleItemsAtSameTimeTest()
        {
            ulong startTime = (ulong)Environment.TickCount64;
            ulong sameTime = startTime + 1;

            var queue = new TimeBasedQueue<string>(startTime, 128);
            Assert.Equal(0, queue.Count);
            Assert.Equal(0, queue.AvailableCount);

            for (int i = 0; i < 1000; i++)
            {
                queue.Add(i.ToString(), sameTime, out int avaiableDelta);
                Assert.Equal(0, avaiableDelta);
                Assert.Equal(0, queue.AvailableCount);
                Assert.Equal(i + 1, queue.Count);
            }

            queue.ValidateInternalCollectionCorrectness();

            int avail = queue.AdvanceTime(sameTime + 1);
            Assert.Equal(1000, avail);
            Assert.Equal(1000, queue.AvailableCount);
            Assert.Equal(1000, queue.Count);

            for (int i = 0; i < 1000; i++)
            {
                Assert.True(queue.TryTake(out var item));
                //Assert.Equal(i.ToString(), item);

                Assert.Equal(1000 - i - 1, queue.AvailableCount);
                Assert.Equal(1000 - i - 1, queue.Count);
            }

            queue.ValidateInternalCollectionCorrectness();
        }


        [Fact]
        public void ClosesTimepointTest()
        {
            ulong startTime = (ulong)Environment.TickCount64;
            ulong itemTime = startTime + 10000;

            var queue = new TimeBasedQueue<string>(startTime, 128);
            queue.Add("abc", itemTime, out int avaiableDelta);

            var nextTimepoint = queue.ClosestTimepoint();
            Assert.NotNull(nextTimepoint);
            Assert.InRange(nextTimepoint.Value, startTime + 1, itemTime);
        }
    }
}
