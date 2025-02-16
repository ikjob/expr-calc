using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.Storage.Api.Exceptions
{
    public class StorageEntityNotFoundException : StorageException
    {
        public StorageEntityNotFoundException() : base("Entity not found") { }
        public StorageEntityNotFoundException(string? message) : base(message) { }
        public StorageEntityNotFoundException(string? message, Exception? innerException) : base(message, innerException) { }
    }
}
