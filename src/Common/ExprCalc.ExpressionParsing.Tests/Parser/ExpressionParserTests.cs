using ExprCalc.ExpressionParsing.Parser;
using ExprCalc.ExpressionParsing.Representation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.ExpressionParsing.Tests.Parser
{
    public class ExpressionParserTests
    {
        public static IEnumerable<object[]> ValidExpressionsWithResults =>
            [
                ["1.E2 + 15 * 3", 145.0],
                ["30 ^ 2 / 100 - -1", 10.0],
                ["((((((1+2)+3)+4)+5)+6)+7)", 28.0],
                ["-1.0", -1.0],
                ["1.5", 1.5],
                ["ln(ln(15))", 0.99622889295139486837451569612192],
                ["-ln(ln(15) + 10)", -2.5422356667537486481598345380761],
                ["33 / 55", 0.6],
                ["(5 ^ 2 - 3 ^ 2) * 2 - 4 ^ 2", 16],
            ];

        public static IEnumerable<object[]> ValidExpressions => ValidExpressionsWithResults.Select(x => new object[] { x[0] });

        [Theory]
        [InlineData("1 + 2", "(1 + 2)")]
        [InlineData("1 + 2 * 3", "(1 + (2 * 3))")]
        [InlineData("1 + 2 ^ 3 ^ 4", "(1 + (2 ^ (3 ^ 4)))")]
        [InlineData("-1 + (1 - - + -  ln(1) * 3)", "((-1) + (1 - ((-(+(-ln(1)))) * 3)))")]
        [InlineData("1 + 2^3^-4 + 10", "((1 + (2 ^ (3 ^ (-4)))) + 10)")]
        [InlineData("1 + 2 * 3 - 4 / (5 + 6)", "((1 + (2 * 3)) - (4 / (5 + 6)))")]
        public void ExpressionParsingTest(string expression, string outputNotation)
        {
            var transformedStr = ExpressionParser.SharedParser.ParseExpression<StringBuildingExpressionNodeFactory, string>(expression, new StringBuildingExpressionNodeFactory());
            Assert.Equal(outputNotation, transformedStr);
        }


        [Theory]
        [MemberData(nameof(ValidExpressions))]
        public void ExpressionValidationForValidExpressionsTest(string expression)
        {
            MathExpression.ValidateExpression(expression);
        }

        [Theory]
        [InlineData("1 ln()")]
        [InlineData("")]
        [InlineData("1 2")]
        [InlineData("abs(10)")]
        [InlineData("ln 10")]
        [InlineData("+")]
        [InlineData("1 1 +")]
        [InlineData("1E99999999999999999999999999999999999999999999")]
        [InlineData("1E+")]
        [InlineData("!")]
        public void ExpressionValidationForInvalidExpressionsTest(string expression)
        {
            Assert.ThrowsAny<ExpressionParserException>(() =>
            {
                MathExpression.ValidateExpression(expression);
            });
        }

        [Theory]
        [MemberData(nameof(ValidExpressionsWithResults))]
        public void ExpressionCalculationForValidExpressionsTest(string expression, double expectedResult)
        {
            var calculatedValue = MathExpression.CalculateExpression(expression, NumberValidationBehaviour.Strict);

            Assert.Equal(expectedResult, calculatedValue, 1E-8);
        }


        [Theory]
        [InlineData("ln(1 - 5)")]
        [InlineData("1 / 0")]
        [InlineData("0 ^ 0")]
        [InlineData("-1 ^ 0.3333")]
        [InlineData("9999999999999 ^ 99999999999999 ^ 9999999999999")]
        public void ExpressionCalculationForInvalidExpressionsTest(string expression)
        {
            Assert.ThrowsAny<ExpressionCalculationException>(() =>
            {
                MathExpression.CalculateExpression(expression, NumberValidationBehaviour.Strict);
            });
        }


        [Theory]
        [MemberData(nameof(ValidExpressionsWithResults))]
        public void ExpressionAstBuildingAndCalculationForValidExpressionsTest(string expression, double expectedResult)
        {
            var ast = MathExpression.BuildExpressionAst(expression);
            double calculatedValue = ast.Calculate(NumberValidationBehaviour.Strict);
            
            Assert.Equal(expectedResult, calculatedValue, 1E-8);
        }


        [Theory]
        [InlineData("ln(-9)")]
        [InlineData("1 / 0")]
        [InlineData("0 ^ 0")]
        [InlineData("-1 ^ 0.3333")]
        [InlineData("9999999999999 ^ 99999999999999 ^ 9999999999999")]
        public void ExpressionAstBuildingAndCalculationForInvalidExpressionsTest(string expression)
        {
            Assert.ThrowsAny<ExpressionCalculationException>(() =>
            {
                var ast = MathExpression.BuildExpressionAst(expression);
                ast.Calculate(NumberValidationBehaviour.Strict);
            });
        }
    }
}
