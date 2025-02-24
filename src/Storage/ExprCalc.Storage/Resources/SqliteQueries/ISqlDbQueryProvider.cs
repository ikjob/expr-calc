using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.Storage.Resources.SqliteQueries
{
    internal interface ISqlDbQueryProvider
    {
        Task InitializeDbIfNeeded(SqliteConnection connection, CancellationToken token);
    }
}
