using ExprCalc.ExpressionParsing.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.ExpressionParsing.Representation.AstNodes
{
    public class BinaryExpressionNode : ExpressionNode
    {
        public BinaryExpressionNode(ExpressionOperationType opType, ExpressionNode arg1, ExpressionNode arg2)
            : base(ExpressionNodeType.BinaryOp)
        {
            if (!opType.IsBinary())
                throw new ArgumentException("Non unary operation type passed to unary expression node");

            OperationType = opType;
            Arg1 = arg1;
            Arg2 = arg2;
        }

        public ExpressionOperationType OperationType { get; }
        public ExpressionNode Arg1 { get; }
        public ExpressionNode Arg2 { get; }

        public override double Calculate(NumberValidationBehaviour numberValidationBehaviour = NumberValidationBehaviour.Strict)
        {
            var calc = new MathOperationsCalculator(numberValidationBehaviour);
            return calc.BinaryOp(OperationType, Arg1.Calculate(numberValidationBehaviour), Arg2.Calculate(numberValidationBehaviour), null);
        }

        public override IEnumerable<ExpressionNode> EnumerateChildNodes()
        {
            yield return Arg1;
            yield return Arg2;
        }

        public override string ToString()
        {
            return OperationType.ToString();
        }
    }
}
