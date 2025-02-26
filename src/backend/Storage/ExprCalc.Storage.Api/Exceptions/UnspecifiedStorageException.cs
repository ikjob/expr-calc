using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.Storage.Api.Exceptions
{
    public class UnspecifiedStorageException : StorageException
    {
        public UnspecifiedStorageException() : base("Unspecified storage error") { }
        public UnspecifiedStorageException(string? message) : base(message) { }
        public UnspecifiedStorageException(string? message, Exception? innerException) : base(message, innerException) { }
    }
}
