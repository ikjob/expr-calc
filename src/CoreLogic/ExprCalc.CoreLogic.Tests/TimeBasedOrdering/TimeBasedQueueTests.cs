using ExprCalc.CoreLogic.Resources.TimeBasedOrdering;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace ExprCalc.CoreLogic.Tests.TimeBasedOrdering
{
    public class TimeBasedQueueTests
    {
        private readonly ITestOutputHelper _output;

        public TimeBasedQueueTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void AddImmediatelyAvailableTest()
        {
            ulong startTime = (ulong)Environment.TickCount64;

            var queue = new TimeBasedQueue<string>(startTime, 128);
            Assert.Equal(0, queue.Count);
            Assert.Equal(0, queue.AvailableCount);
            Assert.Equal(128, queue.Capacity);

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
                Assert.Equal(i.ToString(), item);

                Assert.Equal(1000 - i - 1, queue.AvailableCount);
                Assert.Equal(1000 - i - 1, queue.Count);
            }

            queue.ValidateInternalCollectionCorrectness();
        }


        [Fact]
        public void ClosestTimepointTest()
        {
            ulong startTime = (ulong)Environment.TickCount64;
            ulong itemTime = startTime + 10000;

            var queue = new TimeBasedQueue<string>(startTime, 128);
            queue.Add("abc", itemTime, out int avaiableDelta);

            var nextTimepoint = queue.ClosestTimepoint();
            Assert.NotNull(nextTimepoint);
            Assert.InRange(nextTimepoint.Value, startTime + 1, itemTime);
        }

        [Fact]
        public void NewItemOnTheNextSlotWhileAvancingTest()
        {
            ulong startTime = (ulong)Environment.TickCount64;

            var queue = new TimeBasedQueue<string>(startTime, 128);

            for (int i = 1; i <= 1000; i++)
            {
                queue.Add(i.ToString(), startTime + (ulong)i, out int availableDelta);
                Assert.Equal(0, availableDelta);
                Assert.Equal(0, queue.AvailableCount);
                Assert.False(queue.TryTake(out _));

                availableDelta = queue.AdvanceTime(startTime + (ulong)i);
                Assert.Equal(1, availableDelta);
                Assert.Equal(1, queue.AvailableCount);

                if (i % 20 == 0)
                    queue.ValidateInternalCollectionCorrectness();

                Assert.True(queue.TryTake(out _));
            }
        }


        private void RandomizedTestLogic(int itemCount, int maxItemsGenerationPerStep, int maxAdvancesPerStep, int maxTakesPerStep, int maxNonProducingAdvances, bool doConsistencyValidation)
        {
            const int maxTimeProtectionMs = 2 * 60 * 1000; 

            const int maxItemTimeDelay = 10000000;
            const int maxAdvanceOffset = 1000;

            ulong startTime = (ulong)Environment.TickCount64;

            var queue = new TimeBasedQueue<long>(startTime, 16);
            List<(ulong, long)> referenceList = new List<(ulong, long)>(itemCount + 10);
            var referenceListComparer = Comparer<(ulong, long)>.Create((a, b) => a.Item1.CompareTo(b.Item1));
            List<(ulong takenAt, long val)> takenItemsList = new List<(ulong, long)>(itemCount + 10);
            int totalReleasesReported = 0;
            Random random = new Random();

            int generatedItemsCount = 0;
            int nonProducedAdvances = 0;
            int totalAdvances = 0;
            Stopwatch sw = Stopwatch.StartNew();

            long ticksAdd = 0;
            long ticksAdvance = 0;
            long ticksTake = 0;
            Stopwatch swAdd = new Stopwatch();
            Stopwatch swAdvance = new Stopwatch();
            Stopwatch swTake = new Stopwatch();


            while (generatedItemsCount < itemCount || takenItemsList.Count < referenceList.Count)
            {
                Assert.True(sw.ElapsedMilliseconds < maxTimeProtectionMs);

                if (generatedItemsCount < itemCount)
                {
                    int newStepItems = Math.Min(random.Next(maxItemsGenerationPerStep), itemCount);
                    for (int i = 0; i < newStepItems; i++)
                    {
                        ulong itemTimepoint = queue.CurrentTimepoint + (ulong)random.Next(maxItemTimeDelay);
                        
                        int position = referenceList.BinarySearch((itemTimepoint, generatedItemsCount), referenceListComparer);
                        if (position < 0)
                        {
                            position = ~position;
                        }
                        else
                        {
                            while (position < referenceList.Count && referenceList[position].Item1 == itemTimepoint)
                                position++;
                        }
                        referenceList.Insert(position, (itemTimepoint, generatedItemsCount));

                        swAdd.Restart();
                        queue.Add(generatedItemsCount, itemTimepoint, out _);
                        ticksAdd += swAdd.ElapsedTicks;
                        generatedItemsCount++;
                    }
                }

                int advancesCount = random.Next(maxAdvancesPerStep) + 1;
                for (int i = 0; i < advancesCount; i++)
                {
                    ulong advanceTimepoint = queue.CurrentTimepoint + (ulong)random.Next(maxAdvanceOffset);
                    if (nonProducedAdvances >= maxNonProducingAdvances)
                        advanceTimepoint = queue.ClosestTimepoint() ?? advanceTimepoint;

                    swAdvance.Restart();
                    int newlyAvailableCount = queue.AdvanceTime(advanceTimepoint);
                    ticksAdvance += swAdvance.ElapsedTicks;

                    totalAdvances++;
                    totalReleasesReported += newlyAvailableCount;
                    if (newlyAvailableCount == 0)
                        nonProducedAdvances++;
                    else
                        nonProducedAdvances = 0;
                }

                int expectedToBeAvailable = referenceList.Count(o => o.Item1 <= queue.CurrentTimepoint);
                Assert.Equal(expectedToBeAvailable, takenItemsList.Count + queue.AvailableCount);

                int takesCount = random.Next(maxTakesPerStep);
                for (int i = 0; i < takesCount; i++)
                {
                    swTake.Restart();
                    bool takenFromQueue = queue.TryTake(out var item);
                    ticksTake += swTake.ElapsedTicks;
                    if (!takenFromQueue)
                        break;

                    takenItemsList.Add((queue.CurrentTimepoint, item));
                }

                if (doConsistencyValidation)
                    queue.ValidateInternalCollectionCorrectness();
            }

            _output.WriteLine($"InAdd = {ticksAdd / (Stopwatch.Frequency / 1000)}ms");
            _output.WriteLine($"InAdvance = {ticksAdvance / (Stopwatch.Frequency / 1000)}ms. TotalAdvances = {totalAdvances}");
            _output.WriteLine($"InTake = {ticksTake / (Stopwatch.Frequency / 1000)}ms");

            Assert.Equal(0, queue.Count);
            Assert.Equal(0, queue.AvailableCount);

            Assert.Equal(referenceList.Count, takenItemsList.Count);
            Assert.Equal(referenceList.Count, totalReleasesReported);


            Assert.All(referenceList.Zip(takenItemsList), (item, index) =>
            {
                var (groundTruthAt, groundTruthVal) = item.First;
                var (itemTakenAt, itemVal) = item.Second;

                Assert.Equal(groundTruthVal, itemVal);
                Assert.True(itemTakenAt >= groundTruthAt, "taken not earlier than possible");
            });
        }

        [Theory]
        [InlineData(800, 3, 3, 4, 32, 1, true)]
        [InlineData(5001, 3, 3, 4, 32, 10, false)]
        [InlineData(5002, 5, 2, 10, 1, 5, false)]
        [InlineData(5003, 10000, 1, 10000, 32, 5, false)]
        public void RandomizedQueueTest(int itemCount, int maxItemsGenerationPerStep, int maxAdvancesPerStep, int maxTakesPerStep, int maxNonProducingAdvances, int iterationCount, bool doConsistencyValidation)
        {
            for (int i = 0; i < iterationCount; i++)
            {
                RandomizedTestLogic(itemCount, maxItemsGenerationPerStep, maxAdvancesPerStep, maxTakesPerStep, maxNonProducingAdvances, doConsistencyValidation);
            }
        }
    }
}
