using ExprCalc.Entities;
using ExprCalc.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.CoreLogic.Resources.ExpressionCalculation
{
    internal interface IExpressionCalculator
    {
        Task<CalculationStatus> Calculate(Calculation calculation, CancellationToken softCancellationToken, CancellationToken hardCancellationToken);
    }
}
