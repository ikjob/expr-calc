using ExprCalc.CoreLogic.Configuration;
using ExprCalc.CoreLogic.Instrumentation;
using ExprCalc.CoreLogic.Resources.TimeBasedOrdering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace ExprCalc.CoreLogic.Resources.CalculationsRegistry
{
    internal class TimeBasedCalculationRegistry : ScheduledCalculationsRegistryBase
    {
        private readonly TimeBasedChannel<Item> _channel;

        public TimeBasedCalculationRegistry(int maxCount, ScheduledCalculationsRegistryMetrics metrics)
            : base(maxCount, metrics)
        {
            _channel = new TimeBasedChannel<Item>();
        }

        [ActivatorUtilitiesConstructor]
        public TimeBasedCalculationRegistry(IOptions<CoreLogicConfig> config, InstrumentationContainer instrumentation)
            : this(config.Value.MaxRegisteredCalculationsCount, instrumentation.CalculationsRegistryMetrics)
        {
        }


        protected override void AddNewItemToScheduler(Item item, DateTime availableAfter)
        {
            _channel.Add(item, availableAfter);
        }

        protected override Task<Item> TakeNextScheduledItem(CancellationToken cancellationToken)
        {
            return _channel.Take(cancellationToken);
        }

        protected override bool TryTakeNextScheduledItem(out Item item)
        {
            return _channel.TryTake(out item);
        }
    }
}
