using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.Common.Instrumentation
{
    /// <summary>
    /// Metrics name registry to unify the initialization of the metrics functionality
    /// </summary>
    public class MetricsRegistry
    {
        private readonly HashSet<string> _metricNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public IEnumerable<string> MetricNames => _metricNames;

        public void Add(string metricName)
        {
            _metricNames.Add(metricName);
        }
        public void AddRange(string[] metricNames)
        {
            foreach (var metricName in metricNames)
            {
                _metricNames.Add(metricName);
            }
        }
    }
}
