using ExprCalc.ExpressionParsing.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.ExpressionParsing.Representation
{
    internal readonly struct EmptyNode { }
    internal readonly struct ValidationExpressionNodesFactory : IExpressionNodesFactory<EmptyNode>, IAsyncExpressionNodesFactory<EmptyNode>
    {
        public EmptyNode BinaryOp(ExpressionOperationType opType, EmptyNode left, EmptyNode right) => default;
        public EmptyNode Number(double value) => default;
        public EmptyNode UnaryOp(ExpressionOperationType opType, EmptyNode value) => default;


        public ValueTask<EmptyNode> NumberAsync(double value) => new ValueTask<EmptyNode>(new EmptyNode());
        public ValueTask<EmptyNode> BinaryOpAsync(ExpressionOperationType opType, EmptyNode left, EmptyNode right) => new ValueTask<EmptyNode>(new EmptyNode());
        public ValueTask<EmptyNode> UnaryOpAsync(ExpressionOperationType opType, EmptyNode value) => new ValueTask<EmptyNode>(new EmptyNode());
    }
}
