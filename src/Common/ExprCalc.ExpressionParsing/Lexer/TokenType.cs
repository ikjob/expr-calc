using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.ExpressionParsing.Lexer
{
    internal enum TokenType
    {
        Unknown,
        Identifier,
        Number,
        Plus,
        Minus,
        MultiplicationSign,
        DivisionSign,
        ExponentSign,
        OpeningBracket,
        ClosingBracket
    }
}
