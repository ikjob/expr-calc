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
    internal class QueueBasedCalculationsRegistry : IScheduledCalculationsRegistry, ICalculationProcessingFinisher, ICalculationRegistrySlotFiller
    {
        private readonly struct Item(Calculation calculation, DateTime availableAfter, CancellationTokenSource cancellationTokenSource)
        {
            public readonly Calculation Calculation = calculation;
            public readonly DateTime AvailableAfter = availableAfter;
            public readonly CancellationTokenSource CancellationTokenSource = cancellationTokenSource;
        }

        // ============

        private readonly ConcurrentDictionary<Guid, Item> _calculations;
        private readonly Channel<Item> _channel;
        private readonly int _maxCount;
        private volatile int _count;

        private readonly ScheduledCalculationsRegistryMetrics _metrics;

        public QueueBasedCalculationsRegistry(int maxCount, ScheduledCalculationsRegistryMetrics metrics)
        {
            if (maxCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxCount));

            _calculations = new ConcurrentDictionary<Guid, Item>();
            _channel = Channel.CreateUnbounded<Item>();
            _maxCount = maxCount;
            _count = 0;

            _metrics = metrics;
            _metrics.SetInitialValues(_maxCount);
        }

        [ActivatorUtilitiesConstructor]
        public QueueBasedCalculationsRegistry(IOptions<CoreLogicConfig> config, InstrumentationContainer instrumentation)
            : this(config.Value.MaxRegisteredCalculationsCount, instrumentation.CalculationsRegistryMetrics)
        {
        }

        public bool Contains(Guid id)
        {
            return _calculations.ContainsKey(id);
        }

        public bool TryGetStatus(Guid id, [NotNullWhen(true)] out CalculationStatus? status)
        {
            if (_calculations.TryGetValue(id, out Item item))
            {
                status = item.Calculation.Status;
                return true;
            }

            status = null;
            return false;
        }

        private ValueTask<Item> TakeNextCore(CancellationToken cancellationToken)
        {
            return _channel.Reader.ReadAsync(cancellationToken);
        }
        public async Task<Calculation> TakeNext(CancellationToken cancellationToken)
        {
            var result = await TakeNextCore(cancellationToken);
            if (_calculations.TryRemove(result.Calculation.Id, out _))
            {
                ReleaseReservedSlotCore();
            }
            else
            {
                Debug.Fail("Dictionary should always contain items that was enqueued");
            }
            return result.Calculation;
        }
        public async Task<CalculationProcessingGuard> TakeNextForProcessing(CancellationToken cancellationToken)
        {
            var result = await TakeNextCore(cancellationToken);
            return new CalculationProcessingGuard(result.Calculation, result.CancellationTokenSource.Token, this);

        }

        private bool TryReserveSlotCore()
        {
            int count = _count;
            while (count < _maxCount)
            {
                if (Interlocked.CompareExchange(ref _count, count + 1, count) == count)
                {
                    _metrics.CurrentCount.Add(1);
                    return true;
                }
                count = _count;
            }

            return false;
        }
        private void ReleaseReservedSlotCore()
        {
            int valueAfter = Interlocked.Decrement(ref _count);
            Debug.Assert(valueAfter >= 0);

            _metrics.CurrentCount.Add(-1);
        }

        private void AddCore(Calculation calculation, DateTime availableAfter)
        {
            bool result = false;
            try
            {
                var item = new Item(calculation, availableAfter, new CancellationTokenSource());
                if (!_calculations.TryAdd(calculation.Id, item))
                    throw new DuplicateKeyException("Calculation with the same key is already inside registry");
                if (!_channel.Writer.TryWrite(item))
                    throw new UnexpectedRegistryException("Unable to add calculation to unbounded queue. Should never happen");

                result = true;
            }
            finally
            {
                if (!result)
                {
                    ReleaseReservedSlotCore();
                    _calculations.TryRemove(calculation.Id, out _);
                }
            }
        }

        public bool TryAdd(Calculation calculation, DateTime availableAfter)
        {
            if (!calculation.Status.IsPending())
                throw new ArgumentException("Only calculations in Pending status can be added to the registry", nameof(calculation));

            if (!TryReserveSlotCore())
                return false;

            AddCore(calculation, availableAfter);
            return true;
        }

        public CalculationRegistryReservedSlot TryReserveSlot(Calculation calculation)
        {
            if (TryReserveSlotCore())
            {
                return new CalculationRegistryReservedSlot(this);
            }    
            else
            {
                return default;
            }
        }

        public bool TryCancel(Guid id, User cancelledBy, [NotNullWhen(true)] out CalculationStatusUpdate? status)
        {
            if (_calculations.TryGetValue(id, out var item))
            {
                if (item.Calculation.TryMakeCancelled(cancelledBy))
                {
                    item.CancellationTokenSource.Cancel();
                    status = new CalculationStatusUpdate(id, item.Calculation.UpdatedAt, item.Calculation.Status);
                    return true;
                }
            }

            status = null;
            return false;
        }

        void ICalculationProcessingFinisher.FinishCalculation(Guid id)
        {
            if (_calculations.TryRemove(id, out _))
            {
                ReleaseReservedSlotCore();
            }
            else
            {
                Debug.Fail("Normally FinishCalculation should be called on item inside the registry");
            }
        }

        void ICalculationRegistrySlotFiller.FillSlot(Calculation calculation, DateTime availableAfter)
        {
            if (!calculation.Status.IsPending())
                throw new ArgumentException("Only calculations in Pending status can be added to the registry", nameof(calculation));

            AddCore(calculation, availableAfter);
        }

        void ICalculationRegistrySlotFiller.ReleaseSlot()
        {
            ReleaseReservedSlotCore();
        }
    }
}
