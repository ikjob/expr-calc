using ExprCalc.ExpressionParsing.Parser;
using ExprCalc.ExpressionParsing.Representation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.ExpressionParsing.Tests.Parser
{
    public class ExpressionParserTests
    {
        public static TheoryData<string, double> ValidExpressionsWithResults => new()
            {
                { "1.E2 + 15 * 3", 145.0 },
                { "30 ^ 2 / 100 - -1", 10.0 },
                { "((((((1+2)+3)+4)+5)+6)+7)", 28.0 },
                { "-1.0", -1.0 },
                { "1.5", 1.5 },
                { "ln(ln(15))", 0.99622889295139486837451569612192 },
                { "-ln(ln(15) + 10)", -2.5422356667537486481598345380761 },
                { "33 / 55", 0.6 },
                { "(5 ^ 2 - 3 ^ 2) * 2 - 4 ^ 2", 16 },
                { "-2.5 * -2", 5 },
                { "1.E2 + 1.1E+2 - 100e-2", 209.0 },
            };

        public static TheoryData<string> ValidExpressions => new TheoryData<string>(ValidExpressionsWithResults.Select(o => (string)o[0]));

        public static TheoryData<string> InvalidExpressions => new()
            {
                { "1 ln()" },
                { "" },
                { "1 2" },
                { "abs(10)" },
                { "ln 10" },
                { "+" },
                { "1 1 +" },
                { "1E+" },
                { "!" },
            };

        public static TheoryData<string, ExpressionCalculationErrorType> InvalidCalculationsExpressions => new()
            {
                { "ln(1 - 5)", ExpressionCalculationErrorType.LnFromNegative },
                { "1 / 0", ExpressionCalculationErrorType.DivisionByZero },
                { "0 ^ 0", ExpressionCalculationErrorType.PowZeroZero },
                { "-1 ^ 0.3333", ExpressionCalculationErrorType.NegativeBaseFractionalExponent },
                { "9999999999999 ^ 99999999999999 ^ 9999999999999", ExpressionCalculationErrorType.Overflow },
                { "ln(-9)", ExpressionCalculationErrorType.LnFromNegative },
            };


        [Theory]
        [InlineData("1 + 2", "(1 + 2)")]
        [InlineData("1 + 2 * 3", "(1 + (2 * 3))")]
        [InlineData("1 + 2 ^ 3 ^ 4", "(1 + (2 ^ (3 ^ 4)))")]
        [InlineData("-1 + (1 - - + -  ln(1) * 3)", "((-1) + (1 - ((-(+(-ln(1)))) * 3)))")]
        [InlineData("1 + 2^3^-4 + 10", "((1 + (2 ^ (3 ^ (-4)))) + 10)")]
        [InlineData("1 + 2 * 3 - 4 / (5 + 6)", "((1 + (2 * 3)) - (4 / (5 + 6)))")]
        public void ExpressionParsingTest(string expression, string outputNotation)
        {
            var transformedStr = ExpressionParser.ParseExpression<StringBuildingExpressionNodeFactory, string>(expression, new StringBuildingExpressionNodeFactory());
            Assert.Equal(outputNotation, transformedStr);
        }


        [Theory]
        [MemberData(nameof(ValidExpressions))]
        public void ExpressionValidationForValidExpressionsTest(string expression)
        {
            MathExpression.ValidateExpression(expression);
        }

        [Theory]
        [MemberData(nameof(ValidExpressions))]
        public async Task ExpressionValidationForValidExpressionsTestAsync(string expression)
        {
            await MathExpression.ValidateExpressionAsync(expression);
        }

        [Theory]
        [MemberData(nameof(InvalidExpressions))]
        public void ExpressionValidationForInvalidExpressionsTest(string expression)
        {
            Assert.ThrowsAny<ExpressionParserException>(() =>
            {
                MathExpression.ValidateExpression(expression);
            });
        }

        [Fact]
        public void ExpressionValidationForExpressionsWithInfNumbersTest()
        {
            Assert.ThrowsAny<ExpressionParserException>(() =>
            {
                MathExpression.ValidateExpression("1E99999999999999999999999999999999999999999999", validateNumbersCanBeRepresentedAsDouble: true);
            });
            Assert.ThrowsAny<ExpressionParserException>(() =>
            {
                MathExpression.ValidateExpression("-1E99999999999999999999999999999999999999999999", validateNumbersCanBeRepresentedAsDouble: true);
            });
        }

        [Theory]
        [MemberData(nameof(InvalidExpressions))]
        public async Task ExpressionValidationForInvalidExpressionsTestAsync(string expression)
        {
            await Assert.ThrowsAnyAsync<ExpressionParserException>(async () =>
            {
                await MathExpression.ValidateExpressionAsync(expression);
            });
        }

        [Fact]
        public async Task ExpressionValidationForExpressionsWithInfNumbersTestAsync()
        {
            await Assert.ThrowsAnyAsync<ExpressionParserException>(async () =>
            {
                await MathExpression.ValidateExpressionAsync("1E99999999999999999999999999999999999999999999", validateNumbersCanBeRepresentedAsDouble: true);
            });
            await Assert.ThrowsAnyAsync<ExpressionParserException>(async () =>
            {
                await MathExpression.ValidateExpressionAsync("-1E99999999999999999999999999999999999999999999", validateNumbersCanBeRepresentedAsDouble: true);
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
        [MemberData(nameof(ValidExpressionsWithResults))]
        public async Task ExpressionCalculationForValidExpressionsTestAsync(string expression, double expectedResult)
        {
            var calculatedValue = await MathExpression.CalculateExpressionAsync(expression, NumberValidationBehaviour.Strict);

            Assert.Equal(expectedResult, calculatedValue, 1E-8);
        }


        [Theory]
        [MemberData(nameof(InvalidCalculationsExpressions))]
        public void ExpressionCalculationForInvalidExpressionsTest(string expression, ExpressionCalculationErrorType errorType)
        {
            var err = Assert.ThrowsAny<ExpressionCalculationException>(() =>
            {
                MathExpression.CalculateExpression(expression, NumberValidationBehaviour.Strict);
            });

            Assert.Equal(errorType, err.ErrorType);
        }

        [Theory]
        [MemberData(nameof(InvalidCalculationsExpressions))]
        public async Task ExpressionCalculationForInvalidExpressionsTestAsync(string expression, ExpressionCalculationErrorType errorType)
        {
            var err = await Assert.ThrowsAnyAsync<ExpressionCalculationException>(async () =>
            {
                await MathExpression.CalculateExpressionAsync(expression, NumberValidationBehaviour.Strict);
            });

            Assert.Equal(errorType, err.ErrorType);
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
        [MemberData(nameof(InvalidCalculationsExpressions))]
        public void ExpressionAstBuildingAndCalculationForInvalidExpressionsTest(string expression, ExpressionCalculationErrorType errorType)
        {
            var err = Assert.ThrowsAny<ExpressionCalculationException>(() =>
            {
                var ast = MathExpression.BuildExpressionAst(expression);
                ast.Calculate(NumberValidationBehaviour.Strict);
            });

            Assert.Equal(errorType, err.ErrorType);
        }



        [Fact]
        public void VeryLongExpressionTest()
        {
            const int opNum = 32000;

            StringBuilder builder = new StringBuilder((opNum + 1) * 2);
            for (int i = 0; i < opNum; i++)
            {
                builder.Append("+1");
            }

            double sum = MathExpression.CalculateExpression(builder.ToString(), NumberValidationBehaviour.Strict);
            Assert.Equal(opNum, sum);
        }

        [Fact]
        public async Task VeryLongExpressionTestAsync()
        {
            const int opNum = 32000;

            StringBuilder builder = new StringBuilder((opNum + 1) * 2);
            for (int i = 0; i < opNum; i++)
            {
                builder.Append("+1");
            }

            double sum = await MathExpression.CalculateExpressionAsync(builder.ToString(), NumberValidationBehaviour.Strict);
            Assert.Equal(opNum, sum);
        }
    }
}
