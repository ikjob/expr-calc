using ExprCalc.CoreLogic.Resources.CalculationsRegistry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.CoreLogic.Tests.CalculationsRegistry
{
    public class QueueBasedCalculationRegistryTests
    {
        private static QueueBasedCalculationsRegistry CreateRegistry(int maxCapacity = 100)
        {
            return new QueueBasedCalculationsRegistry(maxCapacity, new Instrumentation.ScheduledCalculationsRegistryMetrics(new System.Diagnostics.Metrics.Meter("test")));
        }

        [Fact]
        public static async Task AddTakeTest()
        {
            await GeneralScheduledCalculationsRegistryTests.AddTakeTest(CreateRegistry());
        }

        [Fact]
        public static void TryTakeTest()
        {
            GeneralScheduledCalculationsRegistryTests.TryTakeTest(CreateRegistry());
        }

        [Fact]
        public static async Task AddOverflowTest()
        {
            await GeneralScheduledCalculationsRegistryTests.AddOverflowTest(CreateRegistry(32), 32);
        }

        [Fact]
        public static async Task KeyUniquenessTest()
        {
            await GeneralScheduledCalculationsRegistryTests.KeyUniquenessTest(CreateRegistry());
        }

        [Fact]
        public static async Task StatusAccessTest()
        {
            await GeneralScheduledCalculationsRegistryTests.StatusAccessTest(CreateRegistry());
        }

        [Fact]
        public static async Task SlotReservationTest()
        {
            await GeneralScheduledCalculationsRegistryTests.SlotReservationTest(CreateRegistry());
        }

        [Fact]
        public static async Task SlotReservationExtendedTest()
        {
            await GeneralScheduledCalculationsRegistryTests.SlotReservationExtendedTest(CreateRegistry(10), 10);
        }

        [Fact]
        public static async Task KeepForProcessingTimeTest()
        {
            await GeneralScheduledCalculationsRegistryTests.KeepForProcessingTimeTest(CreateRegistry(10), 10);
        }

        [Fact]
        public static async Task TakeNextWaitingTest()
        {
            await GeneralScheduledCalculationsRegistryTests.TakeNextWaitingTest(CreateRegistry());
        }

        [Fact]
        public static async Task TryCancelTest()
        {
            await GeneralScheduledCalculationsRegistryTests.TryCancelTest(CreateRegistry());
        }

        [Theory]
        [InlineData(2, 2, 5000, 4, 5)]
        [InlineData(1, 3, 2000, 5, 5)]
        [InlineData(3, 1, 2000, 5, 5)]
        [InlineData(2, 2, 1000000, 0, 0)]
        [InlineData(1, 3, 500000, 0, 0)]
        [InlineData(3, 1, 500000, 0, 0)]
        public static async Task MultithreadTest(int addThreads, int takeThreads, int testItemCount, int addDelay, int takeDelay)
        {
            await GeneralScheduledCalculationsRegistryTests.MultithreadTest(CreateRegistry(), addThreads, takeThreads, testItemCount, addDelay, takeDelay, maxItemDelayMs: 0);
        }
    }
}
