using ExprCalc.Entities;
using ExprCalc.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.CoreLogic.Resources.ExpressionCalculation
{
    internal class ExpressionCalculator : IExpressionCalculator
    {
        private readonly Random _random = new Random();
        private readonly Lock _lock = new Lock();

        public async Task<CalculationStatus> Calculate(Calculation calculation, CancellationToken cancellationToken)
        {
            int delay = 2000;
            lock (_lock)
            {
                delay = _random.Next(1000, 2000);
            }

            await Task.Delay(delay, cancellationToken);
            return CalculationStatus.CreateSuccess(100500);
        }
    }
}
