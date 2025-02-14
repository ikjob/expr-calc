using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.ExpressionParsing.Parser
{
    public interface IExpressionNodesFactory<TNode>
    {
        TNode Number(double value);
        TNode UnaryOp(ExpressionOperationType opType, TNode value);
        TNode BinaryOp(ExpressionOperationType opType, TNode left, TNode right);
    }

    public interface IAsyncExpressionNodesFactory<TNode>
    {
        ValueTask<TNode> NumberAsync(double value);
        ValueTask<TNode> UnaryOpAsync(ExpressionOperationType opType, TNode value);
        ValueTask<TNode> BinaryOpAsync(ExpressionOperationType opType, TNode left, TNode right);
    }
}
