using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.Storage.Tests.DatabaseManagement
{
    internal class TempDirectoryHolder : IDisposable
    {
        public TempDirectoryHolder()
        {
            Directory = Path.Combine(Path.GetTempPath(), "exprcalc_" + Guid.NewGuid().ToString());
        }

        public string Directory { get; }

        public void Dispose()
        {
            System.IO.Directory.Delete(Directory, true);
        }
    }
}
