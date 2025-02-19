using ExprCalc.ExpressionParsing.Parser;
using ExprCalc.ExpressionParsing.Representation.AstNodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.ExpressionParsing.Representation
{
    internal readonly struct AstBuildingExpressionNodesFactory : IExpressionNodesFactory<ExpressionNode>
    {
        public ExpressionNode Number(ReadOnlySpan<char> numberText, int offsetInExpression)
        {
            return new NumberExpressionNode(ExpressionParser.ParseNumberAsDouble(numberText, offsetInExpression));
        }

        public ExpressionNode BinaryOp(ExpressionOperationType opType, int offsetInExpression, ExpressionNode left, ExpressionNode right)
        {
            return new BinaryExpressionNode(opType, left, right);
        }

        public ExpressionNode UnaryOp(ExpressionOperationType opType, int offsetInExpression, ExpressionNode value)
        {
            return new UnaryExpressionNode(opType, value);
        }
    }
}
