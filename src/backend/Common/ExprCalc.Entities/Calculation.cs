using ExprCalc.Entities.Enums;
using ExprCalc.Entities.Exceptions;
using System.Runtime.CompilerServices;

namespace ExprCalc.Entities
{
    /// <summary>
    /// Represent a single calculation with attached expression
    /// </summary>
    public class Calculation
    {
        public const int MaxExpressionLength = 25000;
        private static readonly long _fixedSize = Unsafe.SizeOf<Guid>() + 4 + Unsafe.SizeOf<User>() + Unsafe.SizeOf<DateTime>() * 2 + Unsafe.SizeOf<object>();

        public static Calculation CreateInitial(string expression, User createdBy)
        {
            var createdAt = DateTime.UtcNow;
            return new Calculation(Guid.CreateVersion7(), expression, createdBy, createdAt, createdAt, CalculationStatus.Pending);
        }

        private long _updatedAt;
        private volatile CalculationStatus _status;

        public Calculation(Guid id, string expression, User createdBy, DateTime createdAt, DateTime updatedAt, CalculationStatus status)
        {
            if (string.IsNullOrEmpty(expression))
                throw new ArgumentException("Expression cannot be empty", nameof(expression));
            if (expression.Length > MaxExpressionLength)
                throw new ArgumentException($"Expression length is large than allowed, MaxLength = {MaxExpressionLength}, Actual = {expression.Length}", nameof(expression));
            if (id == Guid.Empty)
                throw new ArgumentException("Id should be specified", nameof(id));

            Id = id;
            Expression = expression;
            CreatedBy = createdBy;
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;

            _status = status;
        }

        public Calculation(Calculation source)
        {
            Id = source.Id;
            Expression = source.Expression;
            CreatedBy = source.CreatedBy;
            CreatedAt = source.CreatedAt;
            UpdatedAt = source.UpdatedAt;

            _status = source.Status;
        }

        public Guid Id { get; private set; }
        public string Expression { get; }
        public User CreatedBy { get; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt
        {
            // Use unsafe hack to guaranty atomic access to UpdatedAt to prevent torn reads
            get { return Unsafe.BitCast<long, DateTime>(Volatile.Read(ref _updatedAt)); } 
            private set { Volatile.Write(ref _updatedAt, Unsafe.BitCast<DateTime, long>(value)); }
        }

        public CalculationStatus Status { get { return _status; } }



        public bool TryChangeStatus(CalculationStatus newStatus, DateTime? updatedAt, out CalculationStatus prevStatus)
        {
            var curStatus = _status;
            while (curStatus.State.IsValidTransition(newStatus.State))
            {
                if (Interlocked.CompareExchange(ref _status, newStatus, curStatus) == curStatus)
                {
                    UpdatedAt = updatedAt ?? DateTime.UtcNow;
                    prevStatus = curStatus;
                    return true;
                }
                // No need to use SpinWait here
                curStatus = _status;
            }

            prevStatus = curStatus;
            return false;
        }
        public bool TryChangeStatus(CalculationStatus newStatus, out CalculationStatus prevStatus)
        {
            return TryChangeStatus(newStatus, null, out prevStatus);
        }

        public bool TryMakeInProgress()
        {
            return _status.State.IsValidTransition(CalculationState.InProgress)
                && TryChangeStatus(CalculationStatus.InProgress, out _);
        }
        public bool TryMakeSuccess(double calculationResult)
        {
            return _status.State.IsValidTransition(CalculationState.Success)
                && TryChangeStatus(CalculationStatus.CreateSuccess(calculationResult), out _);
        }
        public bool TryMakeFailed(CalculationErrorCode errorCode, CalculationErrorDetails errorDetails)
        {
            return _status.State.IsValidTransition(CalculationState.Failed)
                && TryChangeStatus(CalculationStatus.CreateFailed(errorCode, errorDetails), out _);
        }
        public bool TryMakeCancelled(User cancelledBy)
        {
            return _status.State.IsValidTransition(CalculationState.Cancelled)
                && TryChangeStatus(CalculationStatus.CreateCancelled(cancelledBy), out _);
        }

        public void MakeInProgress()
        {
            var curStatus = _status;
            if (!curStatus.State.IsValidTransition(CalculationState.InProgress) || !TryChangeStatus(CalculationStatus.InProgress, out curStatus))
                throw new InvalidStatusTransitionException($"Transition from {curStatus.State} to {CalculationState.InProgress} is not allowed");
        }
        public void MakeSuccess(double calculationResult)
        {
            var curStatus = _status;
            if (!curStatus.State.IsValidTransition(CalculationState.Success) || !TryChangeStatus(CalculationStatus.CreateSuccess(calculationResult), out curStatus))
                throw new InvalidStatusTransitionException($"Transition from {curStatus.State} to {CalculationState.Success} is not allowed");
        }
        public void MakeFailed(CalculationErrorCode errorCode, CalculationErrorDetails errorDetails)
        {
            var curStatus = _status;
            if (!curStatus.State.IsValidTransition(CalculationState.Failed) || !TryChangeStatus(CalculationStatus.CreateFailed(errorCode, errorDetails), out curStatus))
                throw new InvalidStatusTransitionException($"Transition from {curStatus.State} to {CalculationState.Failed} is not allowed");
        }
        public void MakeCancelled(User cancelledBy)
        {
            var curStatus = _status;
            if (!curStatus.State.IsValidTransition(CalculationState.Cancelled) || !TryChangeStatus(CalculationStatus.CreateCancelled(cancelledBy), out curStatus))
                throw new InvalidStatusTransitionException($"Transition from {curStatus.State} to {CalculationState.Cancelled} is not allowed");
        }


        public long GetOccupiedMemoryEstimation()
        {
            return _fixedSize + Expression.Length * sizeof(char) + CreatedBy.GetOccupiedMemoryEstimation();
        }

        public Calculation Clone()
        {
            return new Calculation(this);
        }
        public override string ToString()
        {
            return $"[Id = {Id}, Expression = '{Expression}']";
        }
    }
}
