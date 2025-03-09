using ExprCalc.CoreLogic.Resources.CalculationsRegistry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Quic;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.CoreLogic.Tests.CalculationsRegistry
{
    public class TimeBasedCalculationRegistryTests
    {
        private static TimeBasedCalculationRegistry CreateRegistry(int maxCapacity = 100)
        {
            return new TimeBasedCalculationRegistry(maxCapacity, new Instrumentation.ScheduledCalculationsRegistryMetrics(new System.Diagnostics.Metrics.Meter("test")));
        }

        [Fact]
        public static async Task AddTakeTest()
        {
            using var registry = CreateRegistry();
            await GeneralScheduledCalculationsRegistryTests.AddTakeTest(registry);
        }

        [Fact]
        public static void TryTakeTest()
        {
            using var registry = CreateRegistry();
            GeneralScheduledCalculationsRegistryTests.TryTakeTest(registry);
        }

        [Fact]
        public static async Task AddOverflowTest()
        {
            using var registry = CreateRegistry(32);
            await GeneralScheduledCalculationsRegistryTests.AddOverflowTest(registry, 32);
        }

        [Fact]
        public static async Task KeyUniquenessTest()
        {
            using var registry = CreateRegistry();
            await GeneralScheduledCalculationsRegistryTests.KeyUniquenessTest(registry);
        }

        [Fact]
        public static async Task StatusAccessTest()
        {
            using var registry = CreateRegistry();
            await GeneralScheduledCalculationsRegistryTests.StatusAccessTest(registry);
        }

        [Fact]
        public static async Task SlotReservationTest()
        {
            using var registry = CreateRegistry();
            await GeneralScheduledCalculationsRegistryTests.SlotReservationTest(registry);
        }

        [Fact]
        public static async Task SlotReservationExtendedTest()
        {
            using var registry = CreateRegistry(10);
            await GeneralScheduledCalculationsRegistryTests.SlotReservationExtendedTest(registry, 10);
        }

        [Fact]
        public static async Task KeepForProcessingTimeTest()
        {
            using var registry = CreateRegistry(10);
            await GeneralScheduledCalculationsRegistryTests.KeepForProcessingTimeTest(registry, 10);
        }

        [Fact]
        public static async Task TakeNextWaitingTest()
        {
            using var registry = CreateRegistry();
            await GeneralScheduledCalculationsRegistryTests.TakeNextWaitingTest(registry);
        }

        [Fact]
        public static async Task TryCancelTest()
        {
            using var registry = CreateRegistry();
            await GeneralScheduledCalculationsRegistryTests.TryCancelTest(registry);
        }

        [Fact]
        public static void EnumerateTest()
        {
            GeneralScheduledCalculationsRegistryTests.EnumerateTest(CreateRegistry());
        }

        [Theory]
        [InlineData(2, 2, 5000, 4, 5, 10, 500)]
        [InlineData(3, 1, 2000, 5, 5, 16, 500)]
        [InlineData(1, 3, 2000, 5, 5, 0, 200)]
        [InlineData(3, 1, 2000, 5, 5, 0, 200)]
        [InlineData(2, 2, 100000, 0, 0, 0, 10000)]
        [InlineData(1, 3, 50000, 0, 0, 0, 10000)]
        [InlineData(3, 1, 50000, 0, 0, 0, 10000)]
        public static async Task MultithreadTest(int addThreads, int takeThreads, int testItemCount, int addDelay, int takeDelay, int maxItemDelayMs, int maxCapacity)
        {
            using var registry = CreateRegistry(maxCapacity);
            await GeneralScheduledCalculationsRegistryTests.MultithreadTest(registry, addThreads, takeThreads, testItemCount, addDelay, takeDelay, maxItemDelayMs);
        }
    }
}
