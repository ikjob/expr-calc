using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.CoreLogic.Resources.TimeBasedOrdering
{
    internal class TimeBasedChannel<T> : IDisposable
    {
        /// <summary>
        /// Default capacity of the queue
        /// </summary>
        private const int DefaultCapacity = 32;
        /// <summary>
        /// Time resolution reduction for the <see cref="_queue"/>.
        /// One timestep in <see cref="_queue"/> is equal to 2 ^ <see cref="TimeScaleBitOffset"/> milliseconds
        /// </summary>
        private const int TimeScaleBitOffset = 4;
        /// <summary>
        /// Timer resolution in millseconds. This is the shortes possible step of internal timer
        /// </summary>
        private const int ShortTickIntervalMs = 32;
        /// <summary>
        /// Max number of advances for short ticks that didn't make any item avaiable.
        /// After that threshold the timer switches to long ticks strategy.
        /// </summary>
        private const int MaxNonProducingShortTicks = 32;
        /// <summary>
        /// If delay to the next item in Long ticks mode is less than specified value, then the timer switches to short ticks mode
        /// </summary>
        private const int DelayToNextTickToFallbackToShortTicksMs = ShortTickIntervalMs * 4;

        /// <summary>
        /// Timer operation mode
        /// </summary>
        private enum TimerState
        {
            /// <summary>
            /// No ticks
            /// </summary>
            Suspended,
            /// <summary>
            /// Next tick choosen according to <see cref="TimeBasedQueue{T}.ClosestTimepoint"/>
            /// </summary>
            LongTicks,
            /// <summary>
            /// Period ticks with interval equal to <see cref="ShortTickIntervalMs"/>
            /// </summary>
            ShortTicks
        }


        /// <summary>
        /// Returns current timepoint for the <see cref="_queue"/>.
        /// This timepoint is not in millseconds (scale is applied)
        /// </summary>
        private static ulong GetCurrentTimepoint()
        {
            return (ulong)Environment.TickCount64 >> TimeScaleBitOffset;
        }
        private static ulong GetTimepoint(TimeSpan delayTime)
        {
            if (delayTime <= TimeSpan.Zero)
                return 0;

            // Add 1 at the end to avoid early availability due to number rounding.
            // It is not a problem, because timer resolution is 32ms and queue resolution is 16ms
            return GetCurrentTimepoint() + (((ulong)delayTime.TotalMilliseconds) >> TimeScaleBitOffset) + 1;
        }
        private static ulong GetTimepoint(DateTime dateTime)
        {
            if (dateTime.Kind == DateTimeKind.Utc)
                return GetTimepoint(dateTime - DateTime.UtcNow);
            else
                return GetTimepoint(dateTime - DateTime.Now);
        }

        // ==============

        private readonly TimeBasedQueue<T> _queue;
        private readonly Lock _queueLock;
        private readonly SemaphoreSlim _availableItemsSemaphore;

        private readonly System.Threading.Timer _timer;
        private TimerState _timerState;
        /// <summary>
        /// If timer in <see cref="TimerState.LongTicks"/> then this field stores the next timepoint at which the timer shceduled.
        /// It is important, that this value stores timepoint in terms of <see cref="_queue"/>, not in milliseconds
        /// </summary>
        private ulong _nextLongTickTimepoint;
        /// <summary>
        /// Current number of sequential timer ticks that didn't make any item available
        /// </summary>
        private int _timerNonProducingTicks;


        public TimeBasedChannel(int capacity)
        {
            _queue = new TimeBasedQueue<T>(GetCurrentTimepoint(), capacity);
            _queueLock = new Lock();
            _availableItemsSemaphore = new SemaphoreSlim(0);

            _timer = new Timer(TimeAdvanceHandler, null, Timeout.Infinite, Timeout.Infinite);
            _timerState = TimerState.Suspended;
            _nextLongTickTimepoint = ulong.MaxValue;
            _timerNonProducingTicks = 0;
        }
        public TimeBasedChannel() 
            : this(DefaultCapacity)
        {
        }

        public int Count { get { return _queue.Count; } }
        public int AvailableCount { get { return _queue.AvailableCount; } }

        private void AddInner(T item, ulong timepoint)
        {
            lock (_queueLock)
            {
                _queue.Add(item, timepoint, out int availableCountDelta);
                if (availableCountDelta > 0)
                {
                    _availableItemsSemaphore.Release(availableCountDelta);
                    return;
                }

                // Update timer
                if (_timerState != TimerState.ShortTicks && timepoint < _nextLongTickTimepoint)
                {
                    ReconfigureTimer(GetCurrentTimepoint());
                }
            }
        }
        public void Add(T item, DateTime availableAfter)
        {
            AddInner(item, GetTimepoint(availableAfter));
        }
        public void Add(T item, TimeSpan delay)
        {
            AddInner(item, GetTimepoint(delay));
        }

        private T TakeInner()
        {
            T? result;
            bool success = false;
            _queueLock.Enter();
            try
            {
                if (!_queue.TryTake(out result))
                    throw new InvalidOperationException("Semaphore and queue desynced"); 

                success = true;
            }
            finally
            {
                _queueLock.Exit();
                if (!success)
                    _availableItemsSemaphore.Release();
            }
            return result;
        }
        public bool TryTake([MaybeNullWhen(false)] out T item)
        {
            if (_availableItemsSemaphore.Wait(0))
            {
                item = TakeInner();
                return true;
            }
            else
            {
                item = default;
                return false;
            }
        }
        public async Task<T> Take(CancellationToken token)
        {
            await _availableItemsSemaphore.WaitAsync(token);
            return TakeInner();
        }
        public Task<T> Take()
        {
            return Take(CancellationToken.None);
        }


        /// <summary>
        /// Reconfigure timer: changes operation modes, setup next execution time in long ticks mode, 
        /// suspend timer if it is not needed.
        /// </summary>
        private void ReconfigureTimer(ulong currentTimepoint)
        {
            Debug.Assert(_queueLock.IsHeldByCurrentThread);

            if (_timerState != TimerState.ShortTicks || _timerNonProducingTicks >= MaxNonProducingShortTicks)
            {
                ulong? nextTimepoint = _queue.ClosestTimepoint();
                if (nextTimepoint != null)
                {
                    ulong nextTimepointDeltaMs = 0;
                    if (nextTimepoint.Value > currentTimepoint)
                        nextTimepointDeltaMs = (nextTimepoint.Value - currentTimepoint) << TimeScaleBitOffset;

                    if (nextTimepointDeltaMs <= DelayToNextTickToFallbackToShortTicksMs)
                    {
                        _timer.Change(ShortTickIntervalMs, ShortTickIntervalMs);
                        _nextLongTickTimepoint = ulong.MaxValue;
                        _timerState = TimerState.ShortTicks;
                    }
                    else
                    {
                        if (nextTimepointDeltaMs > int.MaxValue)
                            nextTimepointDeltaMs = int.MaxValue;
                        _timer.Change((int)nextTimepointDeltaMs, Timeout.Infinite);
                        _nextLongTickTimepoint = nextTimepoint.Value;
                        _timerState = TimerState.LongTicks;
                    }
                }
                else
                {
                    _timer.Change(Timeout.Infinite, Timeout.Infinite);
                    _nextLongTickTimepoint = ulong.MaxValue;
                    _timerState = TimerState.Suspended;
                }
            }
        }
        /// <summary>
        /// Timer tick handler
        /// </summary>
        private void TimeAdvanceHandler(object? state)
        {
            lock (_queueLock)
            {
                ulong newTimepoint = GetCurrentTimepoint();

                // Advance queue time
                int availableItemsDelta = _queue.AdvanceTime(newTimepoint);
                if (availableItemsDelta > 0)
                {
                    _availableItemsSemaphore.Release(availableItemsDelta);
                    _timerNonProducingTicks = 0;
                }
                else if (_timerNonProducingTicks < MaxNonProducingShortTicks)
                {
                    _timerNonProducingTicks++;
                }

                // Configure next tick
                ReconfigureTimer(newTimepoint);
            }
        }


        protected virtual void Dispose(bool isUserCall)
        {
            _timer.Dispose();
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
