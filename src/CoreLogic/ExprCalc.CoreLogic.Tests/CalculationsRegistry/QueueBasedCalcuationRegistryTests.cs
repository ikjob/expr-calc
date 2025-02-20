using ExprCalc.CoreLogic.Resources.CalculationsRegistry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.CoreLogic.Tests.CalculationsRegistry
{
    public class QueueBasedCalcuationRegistryTests
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
    }
}
