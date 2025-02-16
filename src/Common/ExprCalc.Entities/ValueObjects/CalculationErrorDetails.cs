using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.Entities.ValueObjects
{
    public record class CalculationErrorDetails
    {
        public const string UnknownFunctionErrorCode = "Unknown function";

        // =======

        public required string ErrorCode { get; init; }
        public int? Offset { get; init; }
        public int? Length { get; init; }
    }
}
