using ExprCalc.ExpressionParsing.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.ExpressionParsing.Tests.Parser
{
    internal class StringBuildingExpressionNodeFactory : IExpressionNodesFactory<string>
    {
        public string BinaryOp(ExpressionOperationType opType, string left, string right)
        {
            return $"({left} {opType.GetStringRepr()} {right})";
        }

        public string Number(double value)
        {
            return value.ToString();
        }

        public string UnaryOp(ExpressionOperationType opType, string value)
        {
            if (opType == ExpressionOperationType.UnaryPlus || opType == ExpressionOperationType.UnaryMinus) 
                return $"({opType.GetStringRepr()}{value})";

            return $"{opType.GetStringRepr()}({value})";
        }
    }
}
