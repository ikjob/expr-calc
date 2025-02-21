using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace ExprCalc.CoreLogic.Instrumentation
{
    internal sealed class InstrumentationContainer
    {
        public const string ActivitySourceName = "ExprCalc.CoreLogic";
        public const string MeterName = "ExprCalc.CoreLogic";

        internal const string MetricsNamePrefix = "core_logic_";

        internal static InstrumentationContainer CreateNull()
        {
            return new InstrumentationContainer(new Meter(new MeterOptions(MeterName)));
        }

        public InstrumentationContainer(IMeterFactory meterFactory)
            : this(meterFactory.Create(new MeterOptions(MeterName)))
        {
        }

        private InstrumentationContainer(Meter meter)
        {
            ActivitySource = new ActivitySource(ActivitySourceName);
            Meter = meter;

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
