using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.ExpressionParsing.Parser
{
    /// <summary>
    /// Base exception for expression parser
    /// </summary>
    public class ExpressionParserException : Exception
    {
        public ExpressionParserException() : base("Invalid expression") { }

        public ExpressionParserException(int offset, int? length) : base("Invalid expression")
        {
            Offset = offset;
            Length = length;
        }
        public ExpressionParserException(string? message, int offset, int? length) : base(message) 
        {
            Offset = offset;
            Length = length;
        }

        public ExpressionParserException(string? message, int offset, int? length, Exception? innerException) : base(message, innerException)
        {
            Offset = offset;
            Length = length;
        }

        public int Offset { get; }
        public int? Length { get; }
    }

    /// <summary>
    /// Lexer found invalid or unsupported lexema
    /// </summary>
    public class InvalidLexemaException : ExpressionParserException
    {
        public InvalidLexemaException(string? message, int offset, int length) : base(message, offset, length) { }
        public InvalidLexemaException(string? message, int offset, int length, Exception? innerException) : base(message, offset, length, innerException) { }
    }

    /// <summary>
    /// Number inside expression cannot be parsed (can be in unexpected format or to large to be represented as double)
    /// </summary>
    public class InvalidNumberException : InvalidLexemaException
    {
        public InvalidNumberException(string? message, int offset, int length) : base(message, length, offset) { }
        public InvalidNumberException(string? message, int offset, int length, Exception? innerException) : base(message, offset, length, innerException) { }
    }

    /// <summary>
    /// Expression contains unknown identifier (unknown function name)
    /// </summary>
    public class UnknownIdentifierException : ExpressionParserException
    {
        public UnknownIdentifierException(string? message, int offset, int length) : base(message, offset, length) { }
        public UnknownIdentifierException(string? message, int offset, int length, Exception? innerException) : base(message, offset, length, innerException) { }
    }

    /// <summary>
    /// Expression has incorrect structure
    /// </summary>
    public class InvalidExpressionException : ExpressionParserException
    {
        public InvalidExpressionException(string? message, int offset, int? length) : base(message, offset, length) { }
        public InvalidExpressionException(string? message, int offset, int? length, Exception? innerException) : base(message, offset, length, innerException) { }
    }

    /// <summary>
    /// Unbalanced brackets or operands in expression
    /// </summary>
    public class UnbalancedExpressionException : InvalidExpressionException
    {
        public UnbalancedExpressionException(string? message, int offset, int? length) : base(message, offset, length) { }
        public UnbalancedExpressionException(string? message, int offset, int? length, Exception? innerException) : base(message, offset, length, innerException) { }
    }
}
