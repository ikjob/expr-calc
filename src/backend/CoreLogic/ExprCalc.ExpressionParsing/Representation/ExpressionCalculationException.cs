using ExprCalc.ExpressionParsing.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.ExpressionParsing.Representation
{
    /// <summary>
    /// Error in expression calculation (e.g. division by zero)
    /// </summary>
    public class ExpressionCalculationException : Exception
    {
        public ExpressionCalculationException() : base("Error in expression calculation") { }
        public ExpressionCalculationException(string? message) : base(message) { }
        public ExpressionCalculationException(string? message, Exception? innerException) : base(message, innerException) { }

        public ExpressionCalculationException(string? message, ExpressionCalculationErrorType errorType, ExpressionOperationType? operationType = null, int? offset = null, int? length = null) 
            : base(message) 
        {
            ErrorType = errorType;
            OperationType = operationType;
            Offset = offset;
            Length = length;
        }
        public ExpressionCalculationException(string? message, ExpressionCalculationErrorType errorType, ExpressionOperationType? operationType, int? offset, int? length, Exception? innerException) 
            : base(message, innerException) 
        {
            ErrorType = errorType;
            OperationType = operationType;
            Offset = offset;
            Length = length;
        }

        public ExpressionCalculationErrorType ErrorType { get; }
        public ExpressionOperationType? OperationType { get; }
        public int? Offset { get; }
        public int? Length { get; }
    }

    /// <summary>
    /// Mathematic error type
    /// </summary>
    public enum ExpressionCalculationErrorType
    {
        Unspecified = 0,
        NumberTooLarge,
        Overflow,
        DivisionByZero,
        LnFromNegative,
        PowZeroZero,
        NegativeBaseFractionalExponent
    }
}
