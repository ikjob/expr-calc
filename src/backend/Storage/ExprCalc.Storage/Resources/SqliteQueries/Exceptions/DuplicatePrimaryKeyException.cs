using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.Storage.Resources.SqliteQueries.Exceptions
{
    internal class DuplicatePrimaryKeyException : Api.Exceptions.StorageDuplicateEntityException
    {
        public DuplicatePrimaryKeyException() : base("Duplicate primary key detected") { }
        public DuplicatePrimaryKeyException(string? message) : base(message) { }
        public DuplicatePrimaryKeyException(string? message, Exception? innerException) : base(message, innerException) { }
    }
}
