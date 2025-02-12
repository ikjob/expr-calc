using ExprCalc.ExpressionParsing.Lexer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.ExpressionParsing.Tests.Lexer
{
    public class IdentifierExtractorTests
    {
        [Theory]
        [InlineData("ln(3)", 0, 2)]
        [InlineData(" Abc12+", 1, 5)]
        [InlineData("a_b", 0, 1)]
        public void ParseIdentifierTest(string text, int offset, int expectedLength)
        {
            var token = IdentifierExtractor.ParseIdentifier(text, ref offset);
            token.ThrowOnError();

            Assert.Equal(expectedLength, token.Length);
        }
    }
}
