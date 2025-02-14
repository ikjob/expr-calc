using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.ExpressionParsing.Lexer
{
    internal class TokenStream : IEnumerator<Token>
    {
        private const string SupportedSymbols = "+-*/^()";

        public readonly struct Enumerable(string text, bool allowErrors) : IEnumerable<Token>
        {
            private readonly string _text = text;
            private readonly bool _allowErrors = allowErrors;

            public TokenStream GetEnumerator()
            {
                return new TokenStream(_text, _allowErrors);
            }

            IEnumerator<Token> IEnumerable<Token>.GetEnumerator() => GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        public static Enumerable EnumerateTokens(string text, bool allowErrors = false)
        {
            return new Enumerable(text, allowErrors);
        }

        // ===========


        private readonly string _text;
        private readonly bool _allowErrors;

        private int _position;
        private Token _current;

        public TokenStream(string text, bool allowErrors)
        {
            _text = text;
            _allowErrors = allowErrors;
            _position = 0;
            _current = Token.CreateEmpty();
        }

        public Token Current => _current;
        object IEnumerator.Current => _current;

        public bool IsError => _current.IsError;
        public bool EndOfStream => _position >= _text.Length;
        public int Position => _position;


        private static void SkipSpaces(string text, ref int position)
        {
            while (position < text.Length && char.IsWhiteSpace(text, position))
                position++;
        }
        private static Token ExtractUnknownSequence(string text, ref int position)
        {
            int initialPos = position;
            while (position < text.Length && 
                    !char.IsWhiteSpace(text[position]) &&
                    !char.IsAsciiLetterOrDigit(text[position]) &&
                    !SupportedSymbols.Contains(text[position]))
            {
                position++;
            }

            return new Token(text, TokenType.Unknown, initialPos, position - initialPos, "Unknown symbol sequence");
        }

        private static Token? NextToken(string text, ref int position)
        {
            SkipSpaces(text, ref position);
            if (position >= text.Length)
                return null;

            switch (text[position])
            {
                case '+':
                    position++;
                    return new Token(text, TokenType.Plus, position - 1, 1);
                case '-':
                    position++;
                    return new Token(text, TokenType.Minus, position - 1, 1);
                case '*':
                    position++;
                    return new Token(text, TokenType.MultiplicationSign, position - 1, 1);
                case '/':
                    position++;
                    return new Token(text, TokenType.DivisionSign, position - 1, 1);
                case '^':
                    position++;
                    return new Token(text, TokenType.ExponentSign, position - 1, 1);
                case '(':
                    position++;
                    return new Token(text, TokenType.OpeningBracket, position - 1, 1);
                case ')':
                    position++;
                    return new Token(text, TokenType.ClosingBracket, position - 1, 1);
                case var ch when char.IsAsciiDigit(ch):
                    return NumberExtractor.ParseNumber(text, ref position);
                case var ch when char.IsAsciiLetter(ch):
                    return IdentifierExtractor.ParseIdentifier(text, ref position);
                default:
                    return ExtractUnknownSequence(text, ref position);
            }
        }

        public bool MoveNext()
        {
            var token = NextToken(_text, ref _position);
            if (token != null && !_allowErrors && token.Value.IsError)
                token.Value.ThrowOnError();

            _current = token ?? Token.CreateEmpty();
            return token != null;
        }

        void IEnumerator.Reset()
        {
            _position = 0;
            _current = Token.CreateEmpty();
        }

        void IDisposable.Dispose()
        {
        }
    }
}
