using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.Storage.Instrumentation
{
    internal sealed class InstrumentationContainer
    {
        public const string ActivitySourceName = "ExprCalc.Storage";
        public const string MeterName = "ExprCalc.Storage";

        internal const string MetricsNamePrefix = "storage_";

        public InstrumentationContainer(IMeterFactory meterFactory)
        {
            ActivitySource = new ActivitySource(ActivitySourceName);
            Meter = meterFactory.Create(new MeterOptions(MeterName));
        }

        internal ActivitySource ActivitySource { get; }
        internal Meter Meter { get; }

        // ======= Metrics ==========
    }
}
