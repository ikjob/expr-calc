using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.CoreLogic.Api.Exceptions
{
    public class TooManyPendingCalculationsException : CoreLogicException
    {
        public TooManyPendingCalculationsException() : base("Too many pending calculations") { }
        public TooManyPendingCalculationsException(string? message) : base(message) { }
        public TooManyPendingCalculationsException(string? message, Exception? innerException) : base(message, innerException) { }
    }
}
