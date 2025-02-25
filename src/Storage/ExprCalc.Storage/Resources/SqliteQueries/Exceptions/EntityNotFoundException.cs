using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.Storage.Resources.SqliteQueries.Exceptions
{
    internal class EntityNotFoundException : Api.Exceptions.StorageEntityNotFoundException
    {
        public EntityNotFoundException() : base("Entity for the specified key not found") { }
        public EntityNotFoundException(string? message) : base(message) { }
        public EntityNotFoundException(string? message, Exception? innerException) : base(message, innerException) { }
    }
}
