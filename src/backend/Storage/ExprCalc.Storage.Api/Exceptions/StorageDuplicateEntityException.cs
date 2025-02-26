using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.Storage.Api.Exceptions
{
    public class StorageDuplicateEntityException : StorageException
    {
        public StorageDuplicateEntityException() : base("Duplicate entity") { }
        public StorageDuplicateEntityException(string? message) : base(message) { }
        public StorageDuplicateEntityException(string? message, Exception? innerException) : base(message, innerException) { }
    }
}
