using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.Entities.Enums
{
    /// <summary>
    /// State of the calculation
    /// </summary>
    public enum CalculationState
    {
        Pending = 0,
        InProgress = 1,
        Cancelled = 2,
        Failed = 3,
        Success = 4
    }

    public static class CalculationStateExtensions
    {
        public static bool IsValidTransition(this CalculationState sourceState, CalculationState targetState)
        {
            return sourceState switch
            {
                CalculationState.Pending => targetState == CalculationState.InProgress || targetState == CalculationState.Cancelled,
                CalculationState.InProgress => targetState == CalculationState.Cancelled || targetState == CalculationState.Failed || targetState == CalculationState.Success,
                CalculationState.Cancelled => false,
                CalculationState.Failed => false,
                CalculationState.Success => false,
                _ => throw new ArgumentException("Unknown calculation state: " + sourceState.ToString())
            };
        }
    }
}
