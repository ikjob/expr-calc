using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.CoreLogic.Services.CalculationsProcessor
{
    internal class BackgroundWorkerStoppedUnexpectedlyException : Exception
    {
        public BackgroundWorkerStoppedUnexpectedlyException() { }
        public BackgroundWorkerStoppedUnexpectedlyException(string? message) : base(message) { }
        public BackgroundWorkerStoppedUnexpectedlyException(string? message, Exception? innerException) : base(message, innerException) { }
    }
}
