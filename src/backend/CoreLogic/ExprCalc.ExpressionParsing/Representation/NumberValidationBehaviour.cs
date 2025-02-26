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
        public static bool IsInfAllowed(this NumberValidationBehaviour behaviour)
        {
            return behaviour != NumberValidationBehaviour.Strict;
        }
        public static bool IsNaNAllowed(this NumberValidationBehaviour behaviour)
        {
            return behaviour == NumberValidationBehaviour.AllowInfAndNaN;
        }

        public static bool IsValidNumber(this NumberValidationBehaviour behaviour, double val)
        {
            return behaviour switch
            {
                NumberValidationBehaviour.Strict when double.IsNaN(val) || double.IsInfinity(val) => false,
                NumberValidationBehaviour.AllowInf when double.IsNaN(val) => false,
                _ => true
            };
        }
        public static void ValidateNumber(this NumberValidationBehaviour behaviour, double val, ExpressionOperationType? operationType = null)
        {
            if (!IsValidNumber(behaviour, val))
            {
                if (operationType != null)
                    throw new ExpressionCalculationException($"Bad operatands for operation {operationType.Value} detected");
                else
                    throw new ExpressionCalculationException($"Bad operatands for operation detected");
            }    
        }
    }
}
