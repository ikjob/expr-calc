using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.CoreLogic.Instrumentation
{
    internal class ScheduledCalculationsRegistryMetrics(Meter meter)
    {
        private const string MetricsNamePrefix = InstrumentationContainer.MetricsNamePrefix + "calculations_registry_";

        public Counter<int> MaxCount { get; } = meter.CreateCounter<int>(MetricsNamePrefix + "max_count", description: "Max number of calculations in registry");
        public UpDownCounter<int> CurrentCount { get; } = meter.CreateUpDownCounter<int>(MetricsNamePrefix + "current_count", description: "Current number of calculations in registry");
        public UpDownCounter<long> CurrentMemory { get; } = meter.CreateUpDownCounter<long>(MetricsNamePrefix + "current_memory", description: "Current memory occupied by calculations in registry");

        public void SetInitialValues(int maxCount)
        {
            MaxCount.Add(maxCount);
            CurrentCount.Add(0);
            CurrentMemory.Add(0);
        }
    }
}
