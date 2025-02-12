using ExprCalc.ExpressionParsing.Lexer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.ExpressionParsing.Tests.Lexer
{
    public class TokenStreamTests
    {
        [Theory]
        [InlineData("100+ 11", 
            new[] { "100", "+", "11" }, 
            new[] { TokenType.Number, TokenType.Plus, TokenType.Number })]
        [InlineData("+cos(15)",
            new[] { "+", "cos", "(", "15", ")" },
            new[] { TokenType.Plus, TokenType.Identifier, TokenType.OpeningBracket, TokenType.Number, TokenType.ClosingBracket })]
        [InlineData("20.0 * ln(10) / 0.1", 
            new[] { "20.0", "*", "ln", "(", "10", ")", "/", "0.1" }, 
            new[] { TokenType.Number, TokenType.MultiplicationSign, TokenType.Identifier, TokenType.OpeningBracket, TokenType.Number, TokenType.ClosingBracket, TokenType.DivisionSign, TokenType.Number })]
        [InlineData("(15+5)*10", 
            new[] { "(", "15", "+", "5", ")", "*", "10" }, 
            new[] { TokenType.OpeningBracket, TokenType.Number, TokenType.Plus, TokenType.Number, TokenType.ClosingBracket, TokenType.MultiplicationSign, TokenType.Number })]
        [InlineData(" (1  + 2) * (3-4.e+1) /2^10", 
            new[] { "(", "1", "+", "2", ")", "*", "(", "3", "-", "4.e+1", ")", "/", "2", "^", "10" }, 
            new[] { TokenType.OpeningBracket, TokenType.Number, TokenType.Plus, TokenType.Number, TokenType.ClosingBracket, TokenType.MultiplicationSign, TokenType.OpeningBracket, TokenType.Number, TokenType.Minus, TokenType.Number, TokenType.ClosingBracket, TokenType.DivisionSign, TokenType.Number, TokenType.ExponentSign, TokenType.Number })]
        internal void TokenEnumerationTest(string expr, string[] expectedTokens, TokenType[] expectedTokenTypes)
        {
            int tokenIndex = 0;
            foreach (var token in TokenStream.EnumerateTokens(expr))
            {
                Assert.False(token.IsError);
                Assert.Equal(expectedTokenTypes[tokenIndex], token.Type);
                Assert.Equal(expectedTokens[tokenIndex], token.GetTokenText());
                tokenIndex++;
            }

            Assert.Equal(expectedTokens.Length, tokenIndex);
        }

        [Theory]
        [InlineData("100u + 11", new[] { "100", "u", "+", "11" })]
        [InlineData("1 $ 10", new[] { "1", "$", "10" })]
        internal void TokenEnumerationBadSequenceTest(string expr, string[] expectedTokens)
        {
            Assert.Throws<TokenizationException>(() =>
            {
                int tokenIndex = 0;
                foreach (var token in TokenStream.EnumerateTokens(expr, allowErrors: false))
                {
                    Assert.Equal(expectedTokens[tokenIndex], token.GetTokenText());
                    tokenIndex++;
                }
            });
        }
    }
}
