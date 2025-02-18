using ExprCalc.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.Entities
{
    public class CalculationStatus
    {
        public static CalculationStatus Pending { get; } = new CalculationStatus(CalculationState.Pending);
        public static CalculationStatus InProgress { get; } = new CalculationStatus(CalculationState.InProgress);
        public static SuccessCalculationStatus CreateSuccess(double calculationResult) => new SuccessCalculationStatus(calculationResult);
        public static FailedCalculationStatus CreateFailed(CalculationErrorCode errorCode, CalculationErrorDetails errorDetails) => new FailedCalculationStatus(errorCode, errorDetails);
        public static CancelledCalculationStatus CreateCancelled(User cancelledBy) => new CancelledCalculationStatus(cancelledBy);

        protected CalculationStatus(CalculationState state)
        {
            State = state;
        }

        public CalculationState State { get; }

        public bool IsPending()
        {
            return State == CalculationState.Pending;
        }
        public bool IsInProgress()
        {
            return State == CalculationState.InProgress;
        }
        public bool IsSuccess([NotNullWhen(true)] out SuccessCalculationStatus? successStatus)
        {
            if (this is SuccessCalculationStatus result)
            {
                Debug.Assert(State == CalculationState.Success);
                successStatus = result;
                return true;
            }
            else
            {
                successStatus = null;
                return false;
            }
        }
        public bool IsFailed([NotNullWhen(true)] out FailedCalculationStatus? failedStatus)
        {
            if (this is FailedCalculationStatus result)
            {
                Debug.Assert(State == CalculationState.Failed);
                failedStatus = result;
                return true;
            }
            else
            {
                failedStatus = null;
                return false;
            }
        }
        public bool IsCancelled([NotNullWhen(true)] out CancelledCalculationStatus? cancelledStatus)
        {
            if (this is CancelledCalculationStatus result)
            {
                Debug.Assert(State == CalculationState.Cancelled);
                cancelledStatus = result;
                return true;
            }
            else
            {
                cancelledStatus = null;
                return false;
            }
        }

        public override string ToString()
        {
            return $"[{State}]";
        }
    }

    public sealed class SuccessCalculationStatus(double calculationResult) : CalculationStatus(CalculationState.Success)
    {
        public double CalculationResult { get; } = calculationResult;

        public override string ToString()
        {
            return $"[Success. Result = {CalculationResult}]";
        }
    }

    public sealed class FailedCalculationStatus(
        CalculationErrorCode errorCode,
        CalculationErrorDetails errorDetails) : CalculationStatus(CalculationState.Failed)
    {
        public CalculationErrorCode ErrorCode { get; } = errorCode;
        public CalculationErrorDetails ErrorDetails { get; } = errorDetails;

        public override string ToString()
        {
            return $"[Failed. ErrorCode = {ErrorCode}]";
        }
    }

    public sealed class CancelledCalculationStatus(User cancelledBy) : CalculationStatus(CalculationState.Cancelled)
    {
        public User CancelledBy { get; } = cancelledBy;

        public override string ToString()
        {
            return $"[Cancelled, By = {CancelledBy}]";
        }
    }
}
