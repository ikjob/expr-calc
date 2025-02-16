using ExprCalc.Entities.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ExprCalc.RestApi.Dto
{
    public record class CalculationErrorDetailsDto
    {
        public required string ErrorCode { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? Offset { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? Length { get; set; }


        public static CalculationErrorDetailsDto FromEntity(CalculationErrorDetails details)
        {
            return new CalculationErrorDetailsDto()
            {
                ErrorCode = details.ErrorCode,
                Offset = details.Offset,
                Length = details.Length
            };
        }
    }
}
