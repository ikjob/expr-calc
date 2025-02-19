using ExprCalc.ExpressionParsing.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.ExpressionParsing.Representation
{
    internal readonly struct CalculationExpressionNodesFactory : IExpressionNodesFactory<double>, IAsyncExpressionNodesFactory<double>
    {
        private readonly MathOperationsCalculator _calculator;

        public CalculationExpressionNodesFactory(NumberValidationBehaviour numberValidationBehaviour)
        {
            _calculator = new MathOperationsCalculator(numberValidationBehaviour);
        }
        public NumberValidationBehaviour NumberValidationBehaviour => _calculator.NumberValidationBehaviour;

        public double Number(ReadOnlySpan<char> numberText, int offsetInExpression)
        {
            return _calculator.ParseNumber(numberText, offsetInExpression);
        }

        public double BinaryOp(ExpressionOperationType opType, int offsetInExpression, double left, double right)
        {
            return _calculator.BinaryOp(opType, left, right, offsetInExpression);
        }

        public double UnaryOp(ExpressionOperationType opType, int offsetInExpression, double value)
        {
            return _calculator.UnaryOp(opType, value, offsetInExpression);
        }

        public ValueTask<double> NumberAsync(ReadOnlySpan<char> numberText, int offsetInExpression)
        {
            try
            {
                return new ValueTask<double>(_calculator.ParseNumber(numberText, offsetInExpression));
            }
            catch (ExpressionCalculationException ex)
            {
                return ValueTask.FromException<double>(ex);
            }
        }

        public ValueTask<double> UnaryOpAsync(ExpressionOperationType opType, int offsetInExpression, double value)
        {
            try
            {
                return new ValueTask<double>(_calculator.UnaryOp(opType, value, offsetInExpression));
            }
            catch (ExpressionCalculationException ex)
            {
                return ValueTask.FromException<double>(ex);
            }
        }

        public ValueTask<double> BinaryOpAsync(ExpressionOperationType opType, int offsetInExpression, double left, double right)
        {
            try
            {
                return new ValueTask<double>(_calculator.BinaryOp(opType, left, right, offsetInExpression));
            }
            catch (ExpressionCalculationException ex)
            {
                return ValueTask.FromException<double>(ex);
            }
        }
    }
}
