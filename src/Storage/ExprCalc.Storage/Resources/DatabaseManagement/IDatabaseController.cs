using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.Storage.Resources.DatabaseManagement
{
    internal interface IDatabaseController : IDisposable, IAsyncDisposable
    {
        Task Init(CancellationToken token);
    }
}
