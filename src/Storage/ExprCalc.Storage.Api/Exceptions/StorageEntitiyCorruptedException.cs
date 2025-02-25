using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.Storage.Api.Exceptions
{
    public class StorageEntitiyCorruptedException : StorageException
    {
        public StorageEntitiyCorruptedException() : base("Entity corrupted") { }
        public StorageEntitiyCorruptedException(string? message) : base(message) { }
        public StorageEntitiyCorruptedException(string? message, Exception? innerException) : base(message, innerException) { }
    }
}
