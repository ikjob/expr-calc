using ExprCalc.Entities.MetadataParams;
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

        PaginatedResult<T> GetCalculationsList<T>(SqliteConnection connection, CalculationFilters filters, PaginationParams pagination, Func<CalculationDbModel, T> transformer);
        PaginatedResult<CalculationDbModel> GetCalculationsList(SqliteConnection connection, CalculationFilters filters, PaginationParams pagination);
        CalculationDbModel GetCalculationById(SqliteConnection connection, Guid id);
        bool ContainsCalculation(SqliteConnection connection, Guid id);

        int ResetNonFinalStateToPending(SqliteConnection connection, DateTime maxCreatedAt, DateTime newUpdatedAt);
        int DeleteCalculations(SqliteConnection connection, DateTime createdBefore);
    }
}
