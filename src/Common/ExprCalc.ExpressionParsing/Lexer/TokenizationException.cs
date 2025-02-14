using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.ExpressionParsing.Lexer
{
    internal class TokenizationException : Exception
    {
        public TokenizationException(string? message, int offset, int length) : base(message)
        {
            Offset = offset;
            Length = length;
        }

        public TokenizationException(string? message, int offset, int length, Exception? innerException) : base(message, innerException)
        {
            Offset = offset;
            Length = length;
        }

        public int Offset { get; }
        public int Length { get; }
    }
}
