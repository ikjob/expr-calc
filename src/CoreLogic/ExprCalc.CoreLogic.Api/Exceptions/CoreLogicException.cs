using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.CoreLogic.Api.Exceptions
{
    public abstract class CoreLogicException : Exception
    {
        public CoreLogicException() { }
        public CoreLogicException(string? message) : base(message) { }
        public CoreLogicException(string? message, Exception? innerException) : base(message, innerException) { }
    }
}
