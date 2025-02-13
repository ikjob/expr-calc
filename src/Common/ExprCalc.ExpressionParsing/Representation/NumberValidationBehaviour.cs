using ExprCalc.ExpressionParsing.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.ExpressionParsing.Representation
{
    public enum NumberValidationBehaviour
    {
        Strict,
        AllowInf,
        AllowInfAndNaN
    }

    public static class NumberValidationBehaviourExtensions
    {
        public static void ValidateNumber(this NumberValidationBehaviour behaviour, double val, ExpressionOperationType? operationType = null)
        {
            bool isValid = behaviour switch
            {
                NumberValidationBehaviour.Strict when double.IsNaN(val) || double.IsInfinity(val) => false,
                NumberValidationBehaviour.AllowInf when double.IsNaN(val) => false,
                _ => true
            };

            if (!isValid)
            {
                if (operationType != null)
                    throw new ExpressionCalculationException($"Bad operatands for operation {operationType.Value} detected");
                else
                    throw new ExpressionCalculationException($"Bad operatands for operation detected");
            }    
        }
    }
}
