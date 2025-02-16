using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.CoreLogic.Instrumentation
{
    internal class MethodMetrics
    {
        internal MethodMetrics(Meter meter, string metricName, string methodName)
        {
            Count = meter.CreateCounter<long>(InstrumentationContainer.MetricsNamePrefix + metricName + "_total", description: $"Number of calls to {methodName} method");
            FailsCount = meter.CreateCounter<long>(InstrumentationContainer.MetricsNamePrefix + metricName + "_fails_total", description: $"Number of calls to {methodName} method ended with error");
        }

        internal Counter<long> Count { get; }
        internal Counter<long> FailsCount { get; }


        internal void AddCall()
        {
            Count.Add(1);
        }
        internal void AddFail()
        {
            FailsCount.Add(1);
        }
    }
}
