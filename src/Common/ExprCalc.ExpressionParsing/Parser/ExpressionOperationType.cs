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
        public static string GetStringRepr(this ExpressionOperationType exprOpType)
        {
            return exprOpType switch
            {
                ExpressionOperationType.Add => "+",
                ExpressionOperationType.Subtract => "-",
                ExpressionOperationType.Multiply => "*",
                ExpressionOperationType.Divide => "/",
                ExpressionOperationType.Exponent => "^",
                ExpressionOperationType.UnaryPlus => "+",
                ExpressionOperationType.UnaryMinus => "-",
                ExpressionOperationType.Ln => "ln",
                _ => throw new UncatchableParserException("Unexpected expression operation type: " + exprOpType.ToString()),
            };
        }

        public static bool IsBinary(this ExpressionOperationType exprOpType)
        {
            return exprOpType <= ExpressionOperationType.Exponent;
        }
        public static bool IsUnary(this ExpressionOperationType exprOpType)
        {
            return exprOpType >= ExpressionOperationType.UnaryPlus;
        }
        public static bool IsFunction(this ExpressionOperationType exprOpType)
        {
            return exprOpType >= ExpressionOperationType.Ln;
        }
    }
}
