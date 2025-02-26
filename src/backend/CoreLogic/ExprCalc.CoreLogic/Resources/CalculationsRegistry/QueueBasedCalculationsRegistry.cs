using ExprCalc.CoreLogic.Configuration;
using ExprCalc.CoreLogic.Instrumentation;
using ExprCalc.Entities;
using ExprCalc.Entities.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace ExprCalc.CoreLogic.Resources.CalculationsRegistry
{
    internal class QueueBasedCalculationsRegistry : ScheduledCalculationsRegistryBase
    {
        private readonly Channel<Item> _channel;

        public QueueBasedCalculationsRegistry(int maxCount, ScheduledCalculationsRegistryMetrics metrics)
            : base(maxCount, metrics)
        {
            _channel = Channel.CreateUnbounded<Item>();
        }

        [ActivatorUtilitiesConstructor]
        public QueueBasedCalculationsRegistry(IOptions<CoreLogicConfig> config, InstrumentationContainer instrumentation)
            : this(config.Value.MaxRegisteredCalculationsCount, instrumentation.CalculationsRegistryMetrics)
        {
        }


        protected override void AddNewItemToScheduler(Item item, TimeSpan delayBeforeExecution)
        {
            if (!_channel.Writer.TryWrite(item))
                throw new UnexpectedRegistryException("Unable to add calculation to unbounded queue. Should never happen");
        }

        protected override Task<Item> TakeNextScheduledItem(CancellationToken cancellationToken)
        {
            return _channel.Reader.ReadAsync(cancellationToken).AsTask();
        }

        protected override bool TryTakeNextScheduledItem(out Item item)
        {
            return _channel.Reader.TryRead(out item);
        }
    }
}
