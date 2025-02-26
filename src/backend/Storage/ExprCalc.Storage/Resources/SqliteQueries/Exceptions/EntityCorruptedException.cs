using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.Storage.Resources.SqliteQueries.Exceptions
{
    internal class EntityCorruptedException : Api.Exceptions.StorageEntitiyCorruptedException
    {
        public EntityCorruptedException() : base("Entity has invalid set of fields or invalid field values") { }
        public EntityCorruptedException(string? message) : base(message) { }
        public EntityCorruptedException(string? message, Exception? innerException) : base(message, innerException) { }
    }
}
