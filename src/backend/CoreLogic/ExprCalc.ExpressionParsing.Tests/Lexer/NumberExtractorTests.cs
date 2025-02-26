using ExprCalc.ExpressionParsing.Lexer;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.ExpressionParsing.Tests.Lexer
{
    public class NumberExtractorTests
    {
        [Theory]
        [InlineData("333", 0, 3)]
        [InlineData(" 15.3", 1, 4)]
        [InlineData("1.+", 0, 2)]
        [InlineData("0.5-", 0, 3)]
        [InlineData("1e10 ", 0, 4)]
        [InlineData("1.e+1 ", 0, 5)]
        [InlineData("0.e-1 ", 0, 5)]
        public void ParseNumberTest(string text, int offset, int expectedLength)
        {
            var token = NumberExtractor.ParseNumber(text, ref offset);
            token.ThrowOnError();

            Assert.Equal(expectedLength, token.Length);
            Assert.True(double.TryParse(token.GetTokenText(), CultureInfo.InvariantCulture, out _));
        }

        [Theory]
        [InlineData("1e ", 0, 2)]
        [InlineData("0.5e-", 0, 5)]
        [InlineData("1u ", 0, 1)]
        public void ParseInvalidNumberTest(string text, int offset, int expectedLength)
        {
            var token = NumberExtractor.ParseNumber(text, ref offset);
            Assert.Equal(expectedLength, token.Length);
            Assert.True(token.IsError);
        }
    }
}
