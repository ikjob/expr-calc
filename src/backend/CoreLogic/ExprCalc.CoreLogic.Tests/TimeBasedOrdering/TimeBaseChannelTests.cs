using ExprCalc.CoreLogic.Resources.TimeBasedOrdering;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.CoreLogic.Tests.TimeBasedOrdering
{
    public class TimeBaseChannelTests
    {
        [Fact]
        public async Task ImmediatelyAvailableItemsAddTakeTest()
        {
            using var deadlockCancellation = new CancellationTokenSource();
            deadlockCancellation.CancelAfter(TimeSpan.FromMinutes(1));

            using var channel = new TimeBasedChannel<string>();
            Assert.Equal(0, channel.AvailableCount);
            Assert.Equal(0, channel.Count);

            channel.Add("abc", DateTime.Now.AddMilliseconds(-100));
            Assert.Equal(1, channel.AvailableCount);
            Assert.Equal(1, channel.Count);

            Assert.True(channel.TryTake(out var takenItem));
            Assert.Equal("abc", takenItem);


            channel.Add("bcd", DateTime.Now.AddMilliseconds(-100));
            Assert.Equal(1, channel.AvailableCount);
            Assert.Equal(1, channel.Count);

            takenItem = await channel.Take(deadlockCancellation.Token);
            Assert.Equal("bcd", takenItem);

            Assert.False(channel.TryTake(out _));
        }

        [Fact]
        public async Task ShortTimeItemsAddTakeTest()
        {
            using var deadlockCancellation = new CancellationTokenSource();
            deadlockCancellation.CancelAfter(TimeSpan.FromMinutes(1));

            using var channel = new TimeBasedChannel<string>();

            DateTime scheduledAt = DateTime.UtcNow.AddMilliseconds(100);
            channel.Add("abc", scheduledAt);

            var item = await channel.Take(deadlockCancellation.Token);
            DateTime takenAt = DateTime.UtcNow;

            Assert.Equal("abc", item);
            Assert.True(takenAt >= scheduledAt);
            Assert.True(takenAt <= scheduledAt.AddMilliseconds(500));
        }

        [Fact]
        public async Task LongTimeItemsAddTakeTest()
        {
            using var deadlockCancellation = new CancellationTokenSource();
            deadlockCancellation.CancelAfter(TimeSpan.FromMinutes(1));

            using var channel = new TimeBasedChannel<string>();

            DateTime scheduledAt = DateTime.UtcNow.AddSeconds(2);
            channel.Add("abc", scheduledAt);

            var item = await channel.Take(deadlockCancellation.Token);
            DateTime takenAt = DateTime.UtcNow;

            Assert.Equal("abc", item);
            // 32ms is a timer resolution
            Assert.True(takenAt >= scheduledAt.AddMilliseconds(-32), $"Delta: {takenAt - scheduledAt}");
            Assert.True(takenAt <= scheduledAt.AddMilliseconds(500));
        }


        [Fact]
        public async Task MultiScheduleTest()
        {
            const int itemsCount = 64;
            const int maxTimeDelay = 4000;

            using var deadlockCancellation = new CancellationTokenSource();
            deadlockCancellation.CancelAfter(TimeSpan.FromMinutes(1));

            using var channel = new TimeBasedChannel<long>();

            Dictionary<long, DateTime> referenceList = new Dictionary<long, DateTime>(itemsCount);
            List<(DateTime takenAt, long val)> takenItemsList = new List<(DateTime, long)>(itemsCount);
            Random random = new Random();

            DateTime startTime = DateTime.UtcNow.AddMilliseconds(100);

            for (int i = 0; i < itemsCount; i++)
            {
                DateTime scheduledAt = startTime.AddMilliseconds(random.Next(maxTimeDelay));
                referenceList.Add(i, scheduledAt);
                channel.Add(i, scheduledAt);
            }

            while (channel.Count > 0)
            {
                var item = await channel.Take(deadlockCancellation.Token);
                takenItemsList.Add((DateTime.UtcNow, item));
            }

            Assert.Equal(0, channel.Count);
            Assert.Equal(0, channel.AvailableCount);

            Assert.Equal(referenceList.Count, takenItemsList.Count);

            foreach (var (takenAt, val) in takenItemsList)
            {
                var groundTruthAt = referenceList[val];
                //32 ms is a timer resolution
                Assert.True(takenAt >= groundTruthAt.AddMilliseconds(-33), "taken earlier than possible");
                Assert.True(takenAt <= groundTruthAt.AddMilliseconds(2000), $"taken too late from expected. Diff = {takenAt - groundTruthAt}");
            }
        }
    }
}
