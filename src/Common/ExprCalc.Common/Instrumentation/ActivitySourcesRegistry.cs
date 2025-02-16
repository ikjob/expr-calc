using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.Common.Instrumentation
{
    /// <summary>
    /// ActivitySource names registry to unify the initialization of the tracing functionality
    /// </summary>
    public class ActivitySourcesRegistry
    {
        private readonly HashSet<string> _activitySourceNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public IEnumerable<string> ActivitySourceNames => _activitySourceNames;

        public void Add(string activitySourceName)
        {
            _activitySourceNames.Add(activitySourceName);
        }
        public void AddRange(string[] activitySourceNames)
        {
            foreach (var activityName in activitySourceNames)
            {
                _activitySourceNames.Add(activityName);
            }
        }
    }
}
