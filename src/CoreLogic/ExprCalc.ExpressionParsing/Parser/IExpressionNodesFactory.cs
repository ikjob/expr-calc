using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.ExpressionParsing.Parser
{
    public interface IExpressionNodesFactory<TNode>
    {
        TNode Number(ReadOnlySpan<char> numberText, int offsetInExpression);
        TNode UnaryOp(ExpressionOperationType opType, int offsetInExpression, TNode value);
        TNode BinaryOp(ExpressionOperationType opType, int offsetInExpression, TNode left, TNode right);
    }

    public interface IAsyncExpressionNodesFactory<TNode>
    {
        ValueTask<TNode> NumberAsync(ReadOnlySpan<char> numberText, int offsetInExpression, CancellationToken cancellationToken);
        ValueTask<TNode> UnaryOpAsync(ExpressionOperationType opType, TNode value, int offsetInExpression, CancellationToken cancellationToken);
        ValueTask<TNode> BinaryOpAsync(ExpressionOperationType opType, TNode left, TNode right, int offsetInExpression, CancellationToken cancellationToken);
    }
}
