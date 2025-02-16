using ExprCalc.CoreLogic.Api.Exceptions;
using ExprCalc.Storage.Api.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.CoreLogic.Helpers
{
    internal static class ExcpetionTranslation
    {
        public static bool TryTranslateStorageException(this StorageException storageException, [NotNullWhen(true)] out Exception? translatedException)
        {
            switch (storageException)
            {
                case StorageEntityNotFoundException:
                    translatedException = new EntityNotFoundException(storageException.Message, storageException);
                    return true;
                default:
                    translatedException = new UnspecifiedCoreLogicException("Unsepcified excpetion. See inner exception for details", storageException);
                    return true;
            }
        }
    }
}
