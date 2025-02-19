using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.Entities
{
    public record class CalculationErrorDetails
    {
        public const string InvalidLexemaErrorCode = "Invalid lexema";
        public const string UnknownFuntionOrIdentifierErrorCode = "Unknown function or identifier";
        public const string UnbalancedExpressionErrorCode = "Unbalanced expression";
        public const string BadExpressionSyntaxErrorCode = "Bad expression syntax";

        public const string UnspecifiedArithmeticProblemErrorCode = "Unspecified arithmetic error";
        public const string NumberToLargeErrorCode = "Number too large";
        public const string OverflowErrorCode = "Number overflow";
        public const string DivisionByZeroErrorCode = "Division by zero";
        public const string LnFromNegativeErrorCode = "Logarithm from negative number";
        public const string PowZeroZeroErrorCode = "Zero raised to the zero power";
        public const string NegativeBaseFractionalExponentErrorCode = "Negative number raised to the fractional power";

        // =======

        public required string ErrorCode { get; init; }
        public int? Offset { get; init; }
        public int? Length { get; init; }
    }
}
