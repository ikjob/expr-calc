using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.CoreLogic.Resources.CalculationsRegistry
{
    internal class DuplicateKeyException : Exception
    {
        public DuplicateKeyException() : base("Duplicate key detected") { }
        public DuplicateKeyException(string? message) : base(message) { }
        public DuplicateKeyException(string? message, Exception? innerException) : base(message, innerException) { }
    }


    internal class UnexpectedRegistryException : Exception
    {
        public UnexpectedRegistryException() : base("Unexpected registry error") { }
        public UnexpectedRegistryException(string? message) : base(message) { }
        public UnexpectedRegistryException(string? message, Exception? innerException) : base(message, innerException) { }
    }
}
