using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.CoreLogic.Instrumentation
{
    internal sealed class InstrumentationContainer
    {
        public const string ActivitySourceName = "ExprCalc.CoreLogic";
        public const string MeterName = "ExprCalc.CoreLogic";

        internal const string MetricsNamePrefix = "core_logic_";

        public InstrumentationContainer(IMeterFactory meterFactory)
        {
            ActivitySource = new ActivitySource(ActivitySourceName);
            Meter = meterFactory.Create(new MeterOptions(MeterName));

            CalculationUseCasesMetrics = new CalculationUseCasesMetrics(Meter);
            CalculationsRegistryMetrics = new ScheduledCalculationsRegistryMetrics(Meter);
            CalculationsProcessingMetrics = new CalculationsProcessingMetrics(Meter);
        }

        internal ActivitySource ActivitySource { get; }
        internal Meter Meter { get; }

        // ======= Metrics ==========

        public CalculationUseCasesMetrics CalculationUseCasesMetrics { get; }
        public ScheduledCalculationsRegistryMetrics CalculationsRegistryMetrics { get; }
        public CalculationsProcessingMetrics CalculationsProcessingMetrics { get; }
    }
}
