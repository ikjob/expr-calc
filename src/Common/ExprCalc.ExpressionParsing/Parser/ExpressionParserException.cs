using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.ExpressionParsing.Parser
{
    public class ExpressionParserException : Exception
    {
        public ExpressionParserException() : base("Invalid expression") { }

        public ExpressionParserException(int offset) : base("Invalid expression") 
        {
            Offset = offset;
        }
        public ExpressionParserException(string? message, int offset) : base(message) 
        {
            Offset = offset;
        }

        public ExpressionParserException(string? message, int offset, Exception? innerException) : base(message, innerException)
        {
            Offset = offset;
        }

        public int Offset { get; }
    }

    public class InvalidLexemaException : ExpressionParserException
    {
        public InvalidLexemaException(string? message, int offset) : base(message, offset) { }
        public InvalidLexemaException(string? message, int offset, Exception? innerException) : base(message, offset, innerException) { }
    }

    public class InvalidNumberException : InvalidLexemaException
    {
        public InvalidNumberException(string? message, int offset) : base(message, offset) { }
        public InvalidNumberException(string? message, int offset, Exception? innerException) : base(message, offset, innerException) { }
    }

    public class UnknownIdentifierException : ExpressionParserException
    {
        public UnknownIdentifierException(string? message, int offset) : base(message, offset) { }
        public UnknownIdentifierException(string? message, int offset, Exception? innerException) : base(message, offset, innerException) { }
    }

    public class InvalidExpressionException : ExpressionParserException
    {
        public InvalidExpressionException(string? message, int offset) : base(message, offset) { }
        public InvalidExpressionException(string? message, int offset, Exception? innerException) : base(message, offset, innerException) { }
    }

    public class UnbalancedExpressionException : InvalidExpressionException
    {
        public UnbalancedExpressionException(string? message, int offset) : base(message, offset) { }
        public UnbalancedExpressionException(string? message, int offset, Exception? innerException) : base(message, offset, innerException) { }
    }
}
