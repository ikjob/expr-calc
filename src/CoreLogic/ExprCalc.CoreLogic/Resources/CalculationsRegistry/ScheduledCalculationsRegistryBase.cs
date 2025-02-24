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
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace ExprCalc.CoreLogic.Resources.CalculationsRegistry
{
    internal abstract class ScheduledCalculationsRegistryBase : IScheduledCalculationsRegistry, ICalculationProcessingFinisher, ICalculationRegistrySlotFiller, IDisposable
    {
        protected readonly struct Item(Calculation calculation, CancellationTokenSource cancellationTokenSource)
        {
            public readonly Calculation Calculation = calculation;
            public readonly CancellationTokenSource CancellationTokenSource = cancellationTokenSource;
        }

        // ============

        private readonly ConcurrentDictionary<Guid, Item> _calculations;
        private readonly int _maxCount;
        private volatile int _count;

        private readonly ScheduledCalculationsRegistryMetrics _metrics;

        public ScheduledCalculationsRegistryBase(int maxCount, ScheduledCalculationsRegistryMetrics metrics)
        {
            if (maxCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxCount));

            _calculations = new ConcurrentDictionary<Guid, Item>();
            _maxCount = maxCount;
            _count = 0;

            _metrics = metrics;
            _metrics.SetInitialValues(_maxCount);
        }


        protected abstract bool TryTakeNextScheduledItem(out Item item);
        protected abstract Task<Item> TakeNextScheduledItem(CancellationToken cancellationToken);
        protected abstract void AddNewItemToScheduler(Item item, DateTime availableAfter);


        public bool IsEmpty { get { return _calculations.IsEmpty; } }

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

        public bool TryTakeNext([NotNullWhen(true)] out Calculation? calculation)
        {
            if (!TryTakeNextScheduledItem(out var result))
            {
                calculation = null;
                return false;
            }

            if (_calculations.TryRemove(result.Calculation.Id, out _))
            {
                ReleaseReservedSlotCore(result.Calculation.GetOccupiedMemory());
            }
            else
            {
                Debug.Fail("Dictionary should always contain items that was enqueued");
            }

            calculation = result.Calculation;
            return true;
        }
        public async Task<Calculation> TakeNext(CancellationToken cancellationToken)
        {
            var result = await TakeNextScheduledItem(cancellationToken);
            if (_calculations.TryRemove(result.Calculation.Id, out _))
            {
                ReleaseReservedSlotCore(result.Calculation.GetOccupiedMemory());
            }
            else
            {
                Debug.Fail("Dictionary should always contain items that was enqueued");
            }
            return result.Calculation;
        }
        public async Task<CalculationProcessingGuard> TakeNextForProcessing(CancellationToken cancellationToken)
        {
            var result = await TakeNextScheduledItem(cancellationToken);
            return new CalculationProcessingGuard(result.Calculation, result.CancellationTokenSource.Token, this);

        }

        private bool TryReserveSlotCore(long memoryCount)
        {
            int count = _count;
            while (count < _maxCount)
            {
                if (Interlocked.CompareExchange(ref _count, count + 1, count) == count)
                {
                    _metrics.CurrentCount.Add(1);
                    _metrics.CurrentMemory.Add(memoryCount);
                    return true;
                }
                count = _count;
            }

            return false;
        }
        private void ReleaseReservedSlotCore(long memoryCount)
        {
            int valueAfter = Interlocked.Decrement(ref _count);
            Debug.Assert(valueAfter >= 0);

            _metrics.CurrentCount.Add(-1);
            _metrics.CurrentMemory.Add(-memoryCount);
        }

        private void AddForAlreadyReservedSlot(Calculation calculation, DateTime availableAfter)
        {
            bool fullSuccess = false;
            bool addedToDictionary = false;
            try
            {
                if (availableAfter.Kind == DateTimeKind.Local)
                    availableAfter = availableAfter.ToUniversalTime();

                var item = new Item(calculation, new CancellationTokenSource());
                if (!_calculations.TryAdd(calculation.Id, item))
                    throw new DuplicateKeyException("Calculation with the same key is already inside registry");
                addedToDictionary = true;

                AddNewItemToScheduler(item, availableAfter);
                fullSuccess = true;
            }
            finally
            {
                if (!fullSuccess)
                {
                    ReleaseReservedSlotCore(calculation.GetOccupiedMemory());
                    if (addedToDictionary)
                        _calculations.TryRemove(calculation.Id, out _);
                }
            }
        }

        public bool TryAdd(Calculation calculation, DateTime availableAfter)
        {
            if (!calculation.Status.IsPending())
                throw new ArgumentException("Only calculations in Pending status can be added to the registry", nameof(calculation));

            if (!TryReserveSlotCore(calculation.GetOccupiedMemory()))
                return false;

            AddForAlreadyReservedSlot(calculation, availableAfter);
            return true;
        }

        public CalculationRegistryReservedSlot TryReserveSlot(Calculation calculation)
        {
            long occupiedMemory = calculation.GetOccupiedMemory();
            if (TryReserveSlotCore(occupiedMemory))
            {
                return new CalculationRegistryReservedSlot(this, occupiedMemory);
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
            if (_calculations.TryRemove(id, out var calculation))
            {
                ReleaseReservedSlotCore(calculation.Calculation.GetOccupiedMemory());
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

            AddForAlreadyReservedSlot(calculation, availableAfter);
        }

        void ICalculationRegistrySlotFiller.ReleaseSlot(long reservedMemory)
        {
            ReleaseReservedSlotCore(reservedMemory);
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
