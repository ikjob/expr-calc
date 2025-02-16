using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.CoreLogic.Api.Exceptions
{
    public class UnspecifiedCoreLogicException : CoreLogicException
    {
        public UnspecifiedCoreLogicException() : base("Unspecified core logic error") { }
        public UnspecifiedCoreLogicException(string? message) : base(message) { }
        public UnspecifiedCoreLogicException(string? message, Exception? innerException) : base(message, innerException) { }
    }
}
