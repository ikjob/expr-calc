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
}
