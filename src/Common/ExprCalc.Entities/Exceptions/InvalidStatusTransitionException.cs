using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.Entities.Exceptions
{
    /// <summary>
    /// Indicates invalid transition of <see cref="Calculation"/> status
    /// </summary>
    public class InvalidStatusTransitionException : InvalidOperationException
    {
        public InvalidStatusTransitionException() : base("Invalid status transition") { }
        public InvalidStatusTransitionException(string? message) : base(message) { }
        public InvalidStatusTransitionException(string? message, Exception? innerException) : base(message, innerException) { }
    }
}
