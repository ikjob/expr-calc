using ExprCalc.Storage.Resources.SqliteQueries.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.Storage.Resources.SqliteQueries
{
    /// <summary>
    /// Provides query execution functions
    /// </summary>
    internal interface ISqlDbCalculationsQueryProvider
    {
        UserDbModel GetOrAddUser(SqliteConnection connection, UserDbModel user);
        CalculationDbModel AddCalculation(SqliteConnection connection, CalculationDbModel calculation);
        bool TryUpdateCalculationStatus(SqliteConnection connection, ICaluclationStatusDbModelView calculationStatus);

        List<T> GetCalculationsList<T>(SqliteConnection connection, Func<CalculationDbModel, T> transformer);
        List<CalculationDbModel> GetCalculationsList(SqliteConnection connection);
        CalculationDbModel GetCalculationById(SqliteConnection connection, Guid id);
    }
}
