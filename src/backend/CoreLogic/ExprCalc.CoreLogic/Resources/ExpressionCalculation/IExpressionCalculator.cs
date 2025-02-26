using ExprCalc.Entities;
using ExprCalc.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.CoreLogic.Resources.ExpressionCalculation
{
    /// <summary>
    /// Perform calculation of expression from <see cref="Calculation"/> and updates its status
    /// </summary>
    internal interface IExpressionCalculator
    {
        /// <summary>
        /// Calculates the expresion in <paramref name="calculation"/>, updates its status and returns as result
        /// </summary>
        /// <param name="calculation">Calculation to be calculated</param>
        /// <param name="softCancellationToken">Cancellation to cancel specific expression calculation</param>
        /// <param name="hardCancellationToken">Cancellation to stop the processing on the application termination (no need to keep states consistent)</param>
        /// <returns>Resulted status</returns>
        Task<CalculationStatus> Calculate(Calculation calculation, CancellationToken softCancellationToken, CancellationToken hardCancellationToken);
    }
}
