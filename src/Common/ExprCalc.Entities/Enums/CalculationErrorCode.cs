using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.Entities.Enums
{
    public enum CalculationErrorCode
    {
        UnexpectedError = 0,
        BadExpressionSyntax = 1,
        ArithmeticError = 2
    }
}
