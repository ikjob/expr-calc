using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.Storage.Api.Exceptions
{
    public abstract class StorageException : Exception
    {
        public StorageException() { }
        public StorageException(string? message) : base(message) { }
        public StorageException(string? message, Exception? innerException) : base(message, innerException) { }
    }
}
