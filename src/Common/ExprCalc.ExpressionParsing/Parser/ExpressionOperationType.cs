using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.ExpressionParsing.Parser
{
    public enum ExpressionOperationType
    {
        Add,
        Subtract,
        Multiply,
        Divide,
        Exponent,

        UnaryPlus,
        UnaryMinus,

        Ln
    }

    public static class ExpressionNodeTypeExtensions
    {
        public static string GetStringRepr(this ExpressionOperationType exprNodeType)
        {
            return exprNodeType switch
            {
                ExpressionOperationType.Add => "+",
                ExpressionOperationType.Subtract => "-",
                ExpressionOperationType.Multiply => "*",
                ExpressionOperationType.Divide => "/",
                ExpressionOperationType.Exponent => "^",
                ExpressionOperationType.UnaryPlus => "+",
                ExpressionOperationType.UnaryMinus => "-",
                ExpressionOperationType.Ln => "ln",
                _ => throw new NotImplementedException(),
            };
        }

        public static bool IsBinary(this ExpressionOperationType exprNodeType)
        {
            return exprNodeType <= ExpressionOperationType.Exponent;
        }
        public static bool IsUnary(this ExpressionOperationType exprNodeType)
        {
            return exprNodeType >= ExpressionOperationType.UnaryPlus;
        }
        public static bool IsFunction(this ExpressionOperationType exprNodeType)
        {
            return exprNodeType >= ExpressionOperationType.Ln;
        }
    }
}
