using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.ExpressionParsing.Representation
{
    /// <summary>
    /// Error in expression calculation (e.g. division by zero)
    /// </summary>
    public class ExpressionCalculationException : Exception
    {
        public ExpressionCalculationException() : base("Error in expression calculation") { }
        public ExpressionCalculationException(string? message) : base(message) { }
        public ExpressionCalculationException(string? message, Exception? innerException) : base(message, innerException) { }
    }
}
