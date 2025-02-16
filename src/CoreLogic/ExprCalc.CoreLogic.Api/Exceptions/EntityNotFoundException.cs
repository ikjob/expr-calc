using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.CoreLogic.Api.Exceptions
{
    public class EntityNotFoundException : CoreLogicException
    {
        public EntityNotFoundException() : base("Entity not found") { }
        public EntityNotFoundException(string? message) : base(message) { }
        public EntityNotFoundException(string? message, Exception? innerException) : base(message, innerException) { }
    }
}
