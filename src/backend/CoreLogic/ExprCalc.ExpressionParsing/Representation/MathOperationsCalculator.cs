using ExprCalc.ExpressionParsing.Parser;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.ExpressionParsing.Representation
{
    public readonly struct MathOperationsCalculator
    {
        public MathOperationsCalculator(NumberValidationBehaviour numberValidationBehaviour)
        {
            NumberValidationBehaviour = numberValidationBehaviour;
        }
        public NumberValidationBehaviour NumberValidationBehaviour { get; }


        public double ParseNumber(ReadOnlySpan<char> numberText, int offsetInExpression)
        {
            double result = ExpressionParser.ParseNumberAsDouble(numberText, offsetInExpression, allowInf: true);
            if (!NumberValidationBehaviour.IsInfAllowed() && double.IsInfinity(result))
                throw new ExpressionCalculationException("Found number that is too large to be parsed", ExpressionCalculationErrorType.NumberTooLarge, null, offsetInExpression, numberText.Length);

            return result;
        }

        private void ValidateOpResultCommon(double result, ExpressionOperationType opType, int? offsetInExpression, int? lengthInExpression)
        {
            if (!NumberValidationBehaviour.IsInfAllowed() && double.IsInfinity(result))
                throw new ExpressionCalculationException($"Overflow on '{opType}' operation detected", ExpressionCalculationErrorType.Overflow, opType, offsetInExpression, lengthInExpression);
            else if (!NumberValidationBehaviour.IsNaNAllowed() && double.IsNaN(result))
                throw new ExpressionCalculationException($"NaN detected on the result of '{opType}' operation", ExpressionCalculationErrorType.Unspecified, opType, offsetInExpression, lengthInExpression);
        }
        public double BinaryOp(ExpressionOperationType opType, double left, double right, int? offsetInExpression)
        {
            double result;
            int? opLength;
            switch (opType)
            {
                case ExpressionOperationType.Add:
                    result = left + right;
                    opLength = 1;
                    break;
                case ExpressionOperationType.Subtract:
                    result = left - right;
                    opLength = 1;
                    break;
                case ExpressionOperationType.Multiply:
                    result = left * right;
                    opLength = 1;
                    break;
                case ExpressionOperationType.Divide:
                    if (right == 0.0)
                        throw new ExpressionCalculationException($"Division by zero detected in {opType} operation", ExpressionCalculationErrorType.DivisionByZero, opType, offsetInExpression, 1);

                    result = left / right;
                    opLength = 1;
                    break;
                case ExpressionOperationType.Exponent:
                    if (left == 0.0 && right == 0.0)
                        throw new ExpressionCalculationException($"Zero to the power of zero detected in {opType} operation", ExpressionCalculationErrorType.PowZeroZero, opType, offsetInExpression, 1);

                    result = Math.Pow(left, right);
                    opLength = 1;
                    if (!NumberValidationBehaviour.IsNaNAllowed() && double.IsNaN(result) && left < 0)
                        throw new ExpressionCalculationException($"Negative number raised to the fraction power by {opType} operation", ExpressionCalculationErrorType.NegativeBaseFractionalExponent, opType, offsetInExpression, 1);
                    break;
                default:
                    throw new ArgumentException("Opearion type is not binary or unknown: " + opType.ToString());
            }

            ValidateOpResultCommon(result, opType, offsetInExpression, opLength);
            return result;
        }

        public double UnaryOp(ExpressionOperationType opType, double value, int? offsetInExpression)
        {
            double result;
            int? opLength;
            switch (opType)
            {
                case ExpressionOperationType.UnaryPlus:
                    result = value;
                    opLength = 1;
                    break;
                case ExpressionOperationType.UnaryMinus:
                    result = -value;
                    opLength = 1;
                    break;
                case ExpressionOperationType.Ln:
                    if (value <= 0.0)
                        throw new ExpressionCalculationException($"Ln from negative number detected in {opType} operation", ExpressionCalculationErrorType.LnFromNegative, opType, offsetInExpression, 2);
                    result = Math.Log(value);
                    opLength = 2;
                    break;
                default:
                    throw new ArgumentException("Opearion type is not unary or unknown: " + opType.ToString());
            }

            ValidateOpResultCommon(result, opType, offsetInExpression, opLength);
            return result;
        }
    }
}
