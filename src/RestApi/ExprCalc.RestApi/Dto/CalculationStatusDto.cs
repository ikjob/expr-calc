using ExprCalc.Entities.Enums;
using ExprCalc.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace ExprCalc.RestApi.Dto
{
    public record class CalculationStatusDto
    {
        public required CalculationState State { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? CalculationResult { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public CalculationErrorCode? ErrorCode { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public CalculationErrorDetailsDto? ErrorDetails { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? CancelledBy { get; set; }


        public static CalculationStatusDto FromEntity(CalculationStatus status)
        {
            var result = new CalculationStatusDto()
            {
                State = status.State
            };

            switch (status.State)
            {
                case CalculationState.Pending:
                case CalculationState.InProgress:
                    break;
                case CalculationState.Success when status.IsSuccess(out var success):
                    result.CalculationResult = success.CalculationResult;
                    break;
                case CalculationState.Failed when status.IsFailed(out var failed):
                    result.ErrorCode = failed.ErrorCode;
                    result.ErrorDetails = CalculationErrorDetailsDto.FromEntity(failed.ErrorDetails);
                    break;
                case CalculationState.Cancelled when status.IsCancelled(out var cancelled):
                    result.CancelledBy = cancelled.CancelledBy.Login;
                    break;
                default:
                    throw new ArgumentException("Unknwon or malformed status: " + status.ToString());
            }

            return result;
        }
    }
}
