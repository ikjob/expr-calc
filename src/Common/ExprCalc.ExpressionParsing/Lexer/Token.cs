using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.ExpressionParsing.Lexer
{
    internal readonly struct Token
    {
        public const int DefaultTokenDisplayLength = 16;

        public static Token CreateEmpty(string? errorDescription = null)
        {
            return new Token("", TokenType.Unknown, 0, 0, errorDescription);
        }


        private readonly string _text;

        public Token(string text, TokenType type, int offset, int length, string? errorDescription = null)
        {
            if (offset < 0 || offset > text.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (offset + length > text.Length)
                throw new ArgumentOutOfRangeException(nameof(length));

            _text = text;
            Type = type;
            Offset = offset;
            Length = length;
            ErrorDescription = errorDescription;
        }

        public TokenType Type { get; }
        public int Offset { get; }
        public int Length { get; }
        public string? ErrorDescription { get; }

        public bool IsError => ErrorDescription != null;

        public ReadOnlySpan<char> GetTokenText()
        {
            return _text.AsSpan(Offset, Length);
        }

        public string GetTokenTextDebug(int maxLength, bool ellipses = true)
        {
            if (Length <= maxLength)
                return _text.Substring(Offset, Length);
            else if (!ellipses)
                return _text.Substring(Offset, maxLength);
            else
                return _text.Substring(Offset, maxLength) + "..";
        }

        public void ThrowOnError()
        {
            if (ErrorDescription != null)
                throw new TokenizationException($"{ErrorDescription}. At [{Offset}, {Length}]. Value = {GetTokenTextDebug(DefaultTokenDisplayLength)}", Offset, Length);
        }

        public override string ToString()
        {
            if (ErrorDescription != null)
                return $"{Type}[{GetTokenTextDebug(DefaultTokenDisplayLength)}]. Error: {ErrorDescription}";

            return $"{Type}[{GetTokenTextDebug(DefaultTokenDisplayLength)}]";
        }
    }
}
