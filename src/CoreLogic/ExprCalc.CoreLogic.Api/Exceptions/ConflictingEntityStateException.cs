using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.CoreLogic.Api.Exceptions
{
    public class ConflictingEntityStateException : CoreLogicException
    {
        public ConflictingEntityStateException() : base("Conflicting entity state") { }
        public ConflictingEntityStateException(string? message) : base(message) { }
        public ConflictingEntityStateException(string? message, Exception? innerException) : base(message, innerException) { }
    }
}
