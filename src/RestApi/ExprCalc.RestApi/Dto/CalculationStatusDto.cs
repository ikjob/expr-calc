using ExprCalc.Entities.Enums;
using ExprCalc.Entities.ValueObjects;
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

        public required DateTime UpdatedAt { get; set; }


        public static CalculationStatusDto FromEntity(Entities.CalculationStatus status)
        {
            return new CalculationStatusDto()
            {
                State = status.State,
                CalculationResult = status.CalculationResult,
                ErrorCode = status.ErrorCode,
                ErrorDetails = status.ErrorDetails != null ? CalculationErrorDetailsDto.FromEntity(status.ErrorDetails) : null,
                CancelledBy = status.CancelledBy?.Login,
                UpdatedAt = status.UpdatedAt
            };
        }
    }
}
