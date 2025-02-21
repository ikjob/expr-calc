using ExprCalc.ExpressionParsing.Parser;
using ExprCalc.ExpressionParsing.Representation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.CoreLogic.Resources.ExpressionCalculation
{
    /// <summary>
    /// Runs expression calculation and applies delays to every operation
    /// </summary>
    internal class CalculatingNodesFactoryWithDelays : IAsyncExpressionNodesFactory<double>
    {
        private readonly MathOperationsCalculator _basicOperationsCalculator;
        private readonly Dictionary<ExpressionOperationType, TimeSpan> _operationsTime;
        public CalculatingNodesFactoryWithDelays(Dictionary<ExpressionOperationType, TimeSpan> operationsTime)
        {
            _basicOperationsCalculator = new MathOperationsCalculator(NumberValidationBehaviour.Strict);
            _operationsTime = operationsTime;
        }

        public ValueTask<double> NumberAsync(ReadOnlySpan<char> numberText, int offsetInExpression, CancellationToken cancellationToken)
        {
            try
            {
                return ValueTask.FromResult(_basicOperationsCalculator.ParseNumber(numberText, offsetInExpression));
            }
            catch (Exception ex)
            {
                return ValueTask.FromException<double>(ex);
            }

        }

        public async ValueTask<double> BinaryOpAsync(ExpressionOperationType opType, double left, double right, int offsetInExpression, CancellationToken cancellationToken)
        {
            if (_operationsTime.TryGetValue(opType, out TimeSpan time) && time > TimeSpan.Zero)
            {
                await Task.Delay(time, cancellationToken);
            }

            return _basicOperationsCalculator.BinaryOp(opType, left, right, offsetInExpression);
        }

        public async ValueTask<double> UnaryOpAsync(ExpressionOperationType opType, double value, int offsetInExpression, CancellationToken cancellationToken)
        {
            if (_operationsTime.TryGetValue(opType, out TimeSpan time) && time > TimeSpan.Zero)
            {
                await Task.Delay(time, cancellationToken);
            }

            return _basicOperationsCalculator.UnaryOp(opType, value, offsetInExpression);
        }
    }
}
