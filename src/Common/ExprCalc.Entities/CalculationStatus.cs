using ExprCalc.Entities.Enums;
using ExprCalc.Entities.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.Entities
{
    /// <summary>
    /// Contains calculation status that is updated during the calculation lifetime
    /// </summary>
    public class CalculationStatus
    {
        public static CalculationStatus CreatePending()
        {
            return new CalculationStatus(CalculationState.Pending, null, null, null, null, DateTime.UtcNow);
        }

        // ===========

        private volatile CalculationState _state;

        public CalculationStatus(
            CalculationState state,
            double? calculationResult,
            CalculationErrorCode? errorCode,
            CalculationErrorDetails? errorDetails,
            User? cancelledBy,
            DateTime updatedAt)
        {
            switch (state)
            {
                case CalculationState.Pending when (calculationResult != null || errorCode != null || errorDetails != null || cancelledBy != null):
                case CalculationState.InProgress when (calculationResult != null || errorCode != null || errorDetails != null || cancelledBy != null):
                case CalculationState.Success when (calculationResult == null || errorCode != null || errorDetails != null || cancelledBy != null):
                case CalculationState.Failed when (calculationResult != null || errorCode == null || errorDetails == null || cancelledBy != null):
                case CalculationState.Cancelled when (calculationResult != null || errorCode != null || errorDetails != null || cancelledBy == null):
                    throw new ArgumentException($"Provided set of values does not match the specified state ({state})");
            }

            _state = state;
            CalculationResult = calculationResult;
            ErrorCode = errorCode;
            ErrorDetails = errorDetails;
            CancelledBy = cancelledBy;
            UpdatedAt = updatedAt;
        }
        public CalculationStatus(CalculationStatus src)
        {
            _state = src.State;
            CalculationResult = src.CalculationResult;
            ErrorCode = src.ErrorCode;
            ErrorDetails = src.ErrorDetails;
            CancelledBy = src.CancelledBy;
            UpdatedAt = src.UpdatedAt;
        }


        public CalculationState State { get { return _state; } }

        public double? CalculationResult { get; private set; }

        public CalculationErrorCode? ErrorCode { get; private set; }
        public CalculationErrorDetails? ErrorDetails { get; private set; }

        public User? CancelledBy { get; private set; }

        public DateTime UpdatedAt { get; private set; }


        public void SetInProgress()
        {
            if (!State.IsValidTransition(CalculationState.InProgress))
                throw new InvalidOperationException($"Unable to transit calculation state from {State} to {CalculationState.InProgress}");

            _state = CalculationState.InProgress;
            UpdatedAt = DateTime.UtcNow;

            CalculationResult = null;
            ErrorCode = null;
            ErrorDetails = null;
            CancelledBy = null;
        }

        public void SetSuccess(double calculationResult)
        {
            if (!State.IsValidTransition(CalculationState.Success))
                throw new InvalidOperationException($"Unable to transit calculation state from {State} to {CalculationState.Success}");

            _state = CalculationState.Success;
            CalculationResult = calculationResult;
            UpdatedAt = DateTime.UtcNow;

            ErrorCode = null;
            ErrorDetails = null;
            CancelledBy = null;
        }

        public void SetFailed(CalculationErrorCode errorCode, CalculationErrorDetails errorDetails)
        {
            if (!State.IsValidTransition(CalculationState.Failed))
                throw new InvalidOperationException($"Unable to transit calculation state from {State} to {CalculationState.Failed}");

            _state = CalculationState.Failed;
            ErrorCode = errorCode;
            ErrorDetails = errorDetails;
            UpdatedAt = DateTime.UtcNow;

            CalculationResult = null;
            CancelledBy = null;
        }

        public void SetCancelled(User cancelledBy)
        {
            if (!State.IsValidTransition(CalculationState.Cancelled))
                throw new InvalidOperationException($"Unable to transit calculation state from {State} to {CalculationState.Cancelled}");

            _state = CalculationState.Cancelled;
            CancelledBy = cancelledBy;
            UpdatedAt = DateTime.UtcNow;

            CalculationResult = null;
            ErrorCode = null;
            ErrorDetails = null;
        }


        public override string ToString()
        {
            return State switch
            {
                CalculationState.Pending => "[Pending]",
                CalculationState.InProgress => "[InProgress]",
                CalculationState.Success => $"[Success. Result = {CalculationResult}]",
                CalculationState.Failed => $"[Failed, ErrorCode = {ErrorCode}]",
                CalculationState.Cancelled => $"[Cancelled, By = {CancelledBy}]",
                _ => throw new InvalidOperationException("Unknown state: " + State.ToString())
            };
        }
    }
}
