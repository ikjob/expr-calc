using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.Storage.Resources.SqliteQueries
{
    internal class SqliteDbQueryProvider : ISqlDbQueryProvider
    {
        public async Task InitializeDbIfNeeded(SqliteConnection connection, CancellationToken token)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = """
                    CREATE TABLE IF NOT EXISTS Users (
                        Id    INTEGER NOT NULL PRIMARY KEY,
                        Login TEXT    NOT NULL UNIQUE
                    )
                    """;

                await command.ExecuteNonQueryAsync(token);
            }

            using (var command = connection.CreateCommand())
            {
                command.CommandText = """
                    CREATE TABLE IF NOT EXISTS Calculations (
                        Id           BLOB    NOT NULL PRIMARY KEY,
                        Expression   TEXT    NOT NULL,
                        CreatedAt    INTEGER NOT NULL,
                        CreatedBy    INTEGER NOT NULL REFERENCES Users(Id),
                        State        INTEGER NOT NULL,
                        CalcResult   DOUBLE  NULL,
                        ErrorCode    INTEGER NULL,
                        ErrorDetails TEXT    NULL,
                        UpdatedAt    INTEGER NOT NULL,
                        CancelledBy  INTEGER NULL REFERENCES Users(Id)
                    )
                    """;

                await command.ExecuteNonQueryAsync(token);
            }
        }
    }
}
