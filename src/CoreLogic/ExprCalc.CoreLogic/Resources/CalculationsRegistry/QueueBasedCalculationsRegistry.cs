using ExprCalc.Entities;
using ExprCalc.Entities.Enums;
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
    internal class QueueBasedCalculationsRegistry : IScheduledCalculationsRegistry, ICalculationProcessingFinisher
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

        public QueueBasedCalculationsRegistry(int maxCount)
        {
            _calculations = new ConcurrentDictionary<Guid, Item>();
            _channel = Channel.CreateBounded<Item>(maxCount);
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

        private async Task<Item> TakeNextCore(CancellationToken cancellationToken)
        {
            while (true)
            {
                var nextItem = await _channel.Reader.ReadAsync(cancellationToken);
                if (!nextItem.CancellationTokenSource.IsCancellationRequested)
                    return nextItem;
                
                if (!_calculations.TryRemove(nextItem.Calculation.Id, out _))
                {
                    Debug.Fail("Dictionary should always contain items that was enqueued");
                }
            }
        }
        public async Task<Calculation> TakeNext(CancellationToken cancellationToken)
        {
            var result = await TakeNextCore(cancellationToken);
            _calculations.TryRemove(result.Calculation.Id, out _);
            return result.Calculation;
        }
        public async Task<CalculationProcessingGuard> TakeNextForProcessing(CancellationToken cancellationToken)
        {
            var result = await TakeNextCore(cancellationToken);
            return new CalculationProcessingGuard(result.Calculation, result.CancellationTokenSource.Token, this);

        }

        public bool TryAdd(Calculation calculation, DateTime availableAfter)
        {
            if (!calculation.Status.IsPending())
                throw new ArgumentException("Only calculations in Pending status can be added to the registry", nameof(calculation));

            return _channel.Writer.TryWrite(new Item(calculation, availableAfter, new CancellationTokenSource()));
        }

        public bool TryCancel(Guid id, User cancelledBy)
        {
            if (_calculations.TryGetValue(id, out var item))
            {
                item.CancellationTokenSource.Cancel();
                return item.Calculation.TryMakeCancelled(cancelledBy);
            }

            return false;
        }

        void ICalculationProcessingFinisher.FinishCalculation(Guid id)
        {
            if (!_calculations.TryRemove(id, out _))
            {
                Debug.Fail("Normally FinishCalculation should be called on item inside the registry");
            }    
        }



        protected virtual void Dispose(bool isUserCall)
        {
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
