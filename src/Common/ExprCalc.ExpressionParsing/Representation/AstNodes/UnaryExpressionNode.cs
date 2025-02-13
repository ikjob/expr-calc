using ExprCalc.ExpressionParsing.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.ExpressionParsing.Representation.AstNodes
{
    public class UnaryExpressionNode : ExpressionNode
    {
        public UnaryExpressionNode(ExpressionOperationType opType, ExpressionNode arg)
            : base(ExpressionNodeType.UnaryOp)
        {
            if (!opType.IsUnary())
                throw new ArgumentException("Non unary operation type passed to unary expression node");

            OperationType = opType;
            Arg = arg;
        }

        public ExpressionOperationType OperationType { get; }
        public ExpressionNode Arg { get; }

        public override double Calculate(NumberValidationBehaviour numberValidationBehaviour = NumberValidationBehaviour.Strict)
        {
            var calc = new CalculationExpressionNodesFactory(numberValidationBehaviour);
            return calc.UnaryOp(OperationType, Arg.Calculate(numberValidationBehaviour));
        }

        public override IEnumerable<ExpressionNode> EnumerateChildNodes()
        {
            yield return Arg;
        }
    }
}
