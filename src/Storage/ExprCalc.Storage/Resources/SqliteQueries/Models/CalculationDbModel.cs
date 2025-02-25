using DotNext;
using ExprCalc.Entities;
using ExprCalc.Entities.Enums;
using ExprCalc.Storage.Resources.SqliteQueries.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.Storage.Resources.SqliteQueries.Models
{
    internal class CalculationDbModel : ICaluclationStatusDbModelView
    {
        public required Guid Id { get; set; }
        public required string Expression { get; set; }
        public required long CreatedAt { get; set; }
        public required long CreatedById { get; set; }
        public required long UpdatedAt { get; set; }
        public required CalculationState State { get; set; }
        
        public double? CalcResult { get; set; }
        public CalculationErrorCode? ErrorCode { get; set; }
        public CalculationErrorDetailsDbModel? ErrorDetails { get; set; }
        public long? CancelledById { get; set; }

        public UserDbModel? CreatedBy { get; set; }
        public UserDbModel? CancelledBy { get; set; }


        public CalculationDbModel Clone()
        {
            return new CalculationDbModel()
            { 
                Id = Id,
                Expression = Expression,
                CreatedAt = CreatedAt,
                CreatedById = CreatedById,
                UpdatedAt = UpdatedAt,
                State = State,
                CalcResult = CalcResult,
                ErrorCode = ErrorCode,
                ErrorDetails = ErrorDetails,
                CancelledById = CancelledById,

                CreatedBy = CreatedBy?.Clone(),
                CancelledBy = CancelledBy?.Clone()
            };
        }

        private static long DateTimeToTimestamp(DateTime dateTime)
        {
            if (dateTime.Kind != DateTimeKind.Utc)
                dateTime = dateTime.ToUniversalTime();

            return (long)(dateTime - DateTime.UnixEpoch).TotalMilliseconds;
        }
        private void FillFromCalculationStatus(CalculationStatus entity)
        {
            State = entity.State;
            switch (entity.State)
            {
                case CalculationState.Pending:
                case CalculationState.InProgress:
                    break;
                case CalculationState.Success when entity.IsSuccess(out var successStatus):
                    CalcResult = successStatus.CalculationResult;
                    break;
                case CalculationState.Failed when entity.IsFailed(out var failedStatus):
                    ErrorCode = failedStatus.ErrorCode;
                    ErrorDetails = CalculationErrorDetailsDbModel.FromEntity(failedStatus.ErrorDetails);
                    break;
                case CalculationState.Cancelled when entity.IsCancelled(out var cancelledStatus):
                    CancelledById = 0;
                    CancelledBy = UserDbModel.FromEntity(cancelledStatus.CancelledBy);
                    break;
                default:
                    throw new ArgumentException("Unknown Calculation state: " + entity.State.ToString());
            }
        }
        public static CalculationDbModel FromEntity(Calculation entity)
        {
            var result = new CalculationDbModel()
            {
                Id = entity.Id,
                Expression = entity.Expression,
                CreatedAt = DateTimeToTimestamp(entity.CreatedAt),
                CreatedById = 0,
                CreatedBy = UserDbModel.FromEntity(entity.CreatedBy),
                UpdatedAt = DateTimeToTimestamp(entity.UpdatedAt),
                State = entity.Status.State
            };

            result.FillFromCalculationStatus(entity.Status);
            return result;
        }
        public static CalculationDbModel FromStatusUpdateEntity(CalculationStatusUpdate statusUpdate)
        {
            var result = new CalculationDbModel()
            {
                Id = statusUpdate.Id,
                Expression = "",
                CreatedAt = 0,
                CreatedById = 0,
                State = statusUpdate.Status.State,
                UpdatedAt = DateTimeToTimestamp(statusUpdate.UpdatedAt)
            };

            result.FillFromCalculationStatus(statusUpdate.Status);
            return result;
        }


        private static DateTime TimestampToDateTime(long timestamp)
        {
            return DateTime.UnixEpoch + TimeSpan.FromMilliseconds(timestamp);
        }
        public CalculationStatus IntoStatusEntity()
        {
            switch (State)
            {
                case CalculationState.Pending:
                    if (CalcResult != null || ErrorCode != null || ErrorDetails != null || CancelledById != null || CancelledBy != null)
                        throw new EntityCorruptedException($"Calculation in {nameof(CalculationState.Pending)} state has fields setted that should not be");
                    return CalculationStatus.Pending;
                case CalculationState.InProgress:
                    if (CalcResult != null || ErrorCode != null || ErrorDetails != null || CancelledById != null || CancelledBy != null)
                        throw new EntityCorruptedException($"Calculation in {nameof(CalculationState.InProgress)} state has fields setted that should not be");
                    return CalculationStatus.Pending;
                case CalculationState.Success:
                    if (CalcResult == null)
                        throw new EntityCorruptedException($"Calculation in {nameof(CalculationState.Success)} does not have calculation result attached");
                    if (ErrorCode != null || ErrorDetails != null || CancelledById != null || CancelledBy != null)
                        throw new EntityCorruptedException($"Calculation in {nameof(CalculationState.Success)} state has fields setted that should not be");
                    return CalculationStatus.CreateSuccess(CalcResult.Value);
                case CalculationState.Failed:
                    if (ErrorCode == null || ErrorDetails == null)
                        throw new EntityCorruptedException($"Calculation in {nameof(CalculationState.Failed)} does not have ErrorCode or ErrorDetails attached");
                    if (CalcResult != null || CancelledById != null || CancelledBy != null)
                        throw new EntityCorruptedException($"Calculation in {nameof(CalculationState.Failed)} state has fields setted that should not be");
                    return CalculationStatus.CreateFailed(ErrorCode.Value, ErrorDetails.Value.IntoEntity());
                case CalculationState.Cancelled:
                    if (CancelledById == null || CancelledBy == null)
                        throw new EntityCorruptedException($"Calculation in {nameof(CalculationState.Cancelled)} does not have CancelledBy attached");
                    if (CalcResult != null || ErrorCode != null || ErrorDetails != null)
                        throw new EntityCorruptedException($"Calculation in {nameof(CalculationState.Cancelled)} state has fields setted that should not be");
                    return CalculationStatus.CreateCancelled(CancelledBy.IntoEntity());
                default:
                    throw new EntityCorruptedException("Unknown entity state: " + State.ToString());
            }
        }
        public Calculation IntoEntity()
        {
            if (CreatedBy == null)
                throw new InvalidOperationException("CreatedBy should be loaded");

            CalculationStatus status = IntoStatusEntity();

            return new Calculation(
                id: Id,
                expression: Expression,
                createdBy: CreatedBy.IntoEntity(),
                createdAt: TimestampToDateTime(CreatedAt),
                updatedAt: TimestampToDateTime(UpdatedAt),
                status: status);
        }
    }
}
