using ExprCalc.ExpressionParsing.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.ExpressionParsing.Representation
{
    internal readonly struct CalculationExpressionNodesFactory : IExpressionNodesFactory<double>
    {
        public CalculationExpressionNodesFactory(NumberValidationBehaviour numberValidationBehaviour)
        {
            NumberValidationBehaviour = numberValidationBehaviour;
        }
        public NumberValidationBehaviour NumberValidationBehaviour { get; }

        public double Number(double value)
        {
            NumberValidationBehaviour.ValidateNumber(value);
            return value;
        }

        private static double StrictPow(double a, double b)
        {
            if (a == 0 && b == 0)
                return double.NaN;

            return Math.Pow(a, b);
        }

        public double BinaryOp(ExpressionOperationType opType, double left, double right)
        {
            var result = opType switch
            {
                ExpressionOperationType.Add => left + right,
                ExpressionOperationType.Subtract => left - right,
                ExpressionOperationType.Multiply => left * right,
                ExpressionOperationType.Divide => left / right,
                ExpressionOperationType.Exponent => StrictPow(left, right),
                _ => throw new Exception()
            };

            NumberValidationBehaviour.ValidateNumber(result, opType);
            return result;
        }

        public double UnaryOp(ExpressionOperationType opType, double value)
        {
            var result = opType switch
            {
                ExpressionOperationType.UnaryPlus => value,
                ExpressionOperationType.UnaryMinus => -value,
                ExpressionOperationType.Ln => Math.Log(value),
                _ => throw new Exception()
            };

            NumberValidationBehaviour.ValidateNumber(result, opType);
            return result;
        }
    }
}
