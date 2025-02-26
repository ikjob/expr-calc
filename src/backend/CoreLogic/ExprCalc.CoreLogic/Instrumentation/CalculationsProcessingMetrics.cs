using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.CoreLogic.Instrumentation
{
    internal class CalculationsProcessingMetrics(Meter meter)
    {
        private const string MetricsNamePrefix = InstrumentationContainer.MetricsNamePrefix + "calculations_processing_service_";

        public Counter<int> ProcessorsCount { get; } = meter.CreateCounter<int>(MetricsNamePrefix + "processor_count", description: "Number of processors in CalculationsProcessingService");
        public Counter<int> ProcessedTotal { get; } = meter.CreateCounter<int>(MetricsNamePrefix + "processed_total", description: "Total number of calculations that was processed");
        public Counter<int> ProcessedSuccessfullyCount { get; } = meter.CreateCounter<int>(MetricsNamePrefix + "processed_sucessfully_total", description: "Total number of calculations that was processed succesfully");
        public Counter<int> ProcessedWithFailureCount { get; } = meter.CreateCounter<int>(MetricsNamePrefix + "processed_failures_total", description: "Total number of calculations that was processed with error");
        public Counter<int> ProcessedWasCancelledCount { get; } = meter.CreateCounter<int>(MetricsNamePrefix + "processed_cancelled_total", description: "Total number of calculations whose processing was cancelled");
        public Histogram<long> ProcessingTimeCounter { get; } = meter.CreateHistogram<long>(MetricsNamePrefix + "processing_time_ms", description: "Time of processing");
        

        public void SetInitialValues(int processorCount)
        {
            ProcessorsCount.Add(processorCount);
            ProcessedTotal.Add(0);
            ProcessedSuccessfullyCount.Add(0);
            ProcessedWithFailureCount.Add(0);
            ProcessedWasCancelledCount.Add(0);
        }
    }
}
