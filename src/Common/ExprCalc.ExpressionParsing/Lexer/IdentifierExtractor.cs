using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.ExpressionParsing.Lexer
{
    internal static class IdentifierExtractor
    {
        public static Token ParseIdentifier(string text, ref int position)
        {
            if (position >= text.Length)
                throw new ArgumentOutOfRangeException(nameof(position), "Initial position should be within the text");

            if (!char.IsAsciiLetter(text[position]))
                throw new ArgumentException($"Symbol at initial position for identifier should be an ascii letter. Found: {text[position]}");

            int initialPos = position;
            while (position < text.Length && (char.IsAsciiLetterOrDigit(text[position]) || text[position] == '_'))
                position++;

            return new Token(text, TokenType.Identifier, initialPos, position - initialPos);
        }
    }
}
