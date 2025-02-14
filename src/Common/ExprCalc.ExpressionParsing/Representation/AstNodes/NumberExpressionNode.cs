using ExprCalc.ExpressionParsing.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.ExpressionParsing.Representation.AstNodes
{
    public class NumberExpressionNode : ExpressionNode
    {
        public NumberExpressionNode(double value) 
            : base(ExpressionNodeType.Number)
        {
            Value = value;
        }

        public double Value { get; }

        public override double Calculate(NumberValidationBehaviour numberValidationBehaviour = NumberValidationBehaviour.Strict)
        {
            numberValidationBehaviour.ValidateNumber(Value);
            return Value;
        }

        public override IEnumerable<ExpressionNode> EnumerateChildNodes()
        {
            yield break;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
