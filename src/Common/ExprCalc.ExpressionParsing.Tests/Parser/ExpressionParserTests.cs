using ExprCalc.ExpressionParsing.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.ExpressionParsing.Tests.Parser
{
    public class ExpressionParserTests
    {
        [Theory]
        [InlineData("1 + 2", "(1 + 2)")]
        [InlineData("-1 + (1 - - + -  ln(1) * 3)", "((-1) + (1 - ((-(+(-ln(1)))) * 3)))")]
        [InlineData("1 + 2^3^-4 + 10", "((1 + (2 ^ (3 ^ (-4)))) + 10)")]
        [InlineData("1 + 2 * 3 - 4 / (5 + 6)", "((1 + (2 * 3)) - (4 / (5 + 6)))")]
        public void ExpressionParsingTest(string expression, string outputNotation)
        {
            var parser = new ExpressionParser<StringBuildingExpressionNodeFactory, string>(new StringBuildingExpressionNodeFactory());
            var transformedStr = parser.ParseExpression(expression);
            Assert.Equal(outputNotation, transformedStr);
        }
    }
}
