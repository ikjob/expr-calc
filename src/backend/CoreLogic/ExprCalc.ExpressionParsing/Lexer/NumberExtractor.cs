using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.ExpressionParsing.Lexer
{
    internal static class NumberExtractor
    {
        private const char DecimalSeparator = '.';

        public static Token ParseNumber(string text, ref int position)
        {
            if (position >= text.Length)
                throw new ArgumentOutOfRangeException(nameof(position), "Initial position should be within the text");

            if (!char.IsAsciiDigit(text[position]))
                throw new ArgumentException($"Symbol at initial position for identifier should be an ascii digit. Found: {text[position]}");

            int initialPos = position;

            // Capture digits before '.'
            while (position < text.Length && char.IsAsciiDigit(text[position]))
                position++;

            // Parse decimal separator
            if (position < text.Length && text[position] == DecimalSeparator)
            {
                position++;

                // Parse digits after '.'
                while (position < text.Length && char.IsAsciiDigit(text[position]))
                    position++;
            }

            // Parse exponent symbol
            if (position < text.Length && (text[position] == 'E' || text[position] == 'e'))
            {
                position++;
                if (position < text.Length && (text[position] == '-' || text[position] == '+'))
                    position++;

                int posBeforeExpDigits = position;
                // Parse digits after 'E'
                while (position < text.Length && char.IsAsciiDigit(text[position]))
                    position++;

                // Incorrect format
                if (posBeforeExpDigits == position)
                    return new Token(text, TokenType.Number, initialPos, position - initialPos, "Digits in exponent expected");
            }

            // Disallow numbers ending with letter
            if (position < text.Length && char.IsLetterOrDigit(text[position]))
                return new Token(text, TokenType.Number, initialPos, position - initialPos, "Number should not end with any letter");

            return new Token(text, TokenType.Number, initialPos, position - initialPos);
        }
    }
}
