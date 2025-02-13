using ExprCalc.ExpressionParsing.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.ExpressionParsing.Representation.AstNodes
{
    public abstract class ExpressionNode
    {
        public ExpressionNode(ExpressionNodeType nodeType)
        {
            NodeType = nodeType;
        }

        public ExpressionNodeType NodeType { get; }

        public abstract double Calculate(NumberValidationBehaviour numberValidationBehaviour = NumberValidationBehaviour.Strict);
        public abstract IEnumerable<ExpressionNode> EnumerateChildNodes();
    }
}
