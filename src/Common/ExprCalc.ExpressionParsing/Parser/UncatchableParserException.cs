using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.ExpressionParsing.Parser
{
    internal class UncatchableParserException : Exception
    {
        public UncatchableParserException(string message) : base(message) { }
    }
}
