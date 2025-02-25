using ExprCalc.Entities;
using ExprCalc.Entities.Enums;
using ExprCalc.Entities.MetadataParams;
using ExprCalc.Storage.Resources.SqliteQueries.Exceptions;
using ExprCalc.Storage.Resources.SqliteQueries.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.Storage.Resources.SqliteQueries
{
    /// <summary>
    /// Provides query execution logic
    /// </summary>
    internal class SqliteDbQueryProvider : ISqlDbInitializationQueryProvider, ISqlDbCalculationsQueryProvider
    {
        public void InitializeDbIfNeeded(SqliteConnection connection)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = """
                    CREATE TABLE IF NOT EXISTS Users (
                        Id    INTEGER NOT NULL PRIMARY KEY,
                        Login TEXT    NOT NULL UNIQUE
                    )
                    """;

                command.ExecuteNonQuery();
            }

            using (var command = connection.CreateCommand())
            {
                command.CommandText = """
                    CREATE TABLE IF NOT EXISTS Calculations (
                        Id           BLOB    NOT NULL PRIMARY KEY,
                        Expression   TEXT    NOT NULL,
                        CreatedAt    INTEGER NOT NULL,
                        CreatedBy    INTEGER NOT NULL REFERENCES Users(Id),
                        UpdatedAt    INTEGER NOT NULL,
                        State        INTEGER NOT NULL,
                        CalcResult   DOUBLE  NULL,
                        ErrorCode    INTEGER NULL,
                        ErrorDetails TEXT    NULL,
                        CancelledBy  INTEGER NULL REFERENCES Users(Id)
                    )
                    """;

                command.ExecuteNonQuery();
            }

            using (var command = connection.CreateCommand())
            {
                command.CommandText = $"""
                    CREATE INDEX IF NOT EXISTS idx_Calculations_CreatedAt_UpdatedAt ON Calculations (
                        CreatedAt DESC,
                        UpdatedAt
                    );

                    CREATE INDEX IF NOT EXISTS idx_Calculations_CreatedAt_CreatedBy ON Calculations (
                        CreatedAt DESC,
                        CreatedBy
                    );

                    CREATE INDEX IF NOT EXISTS idx_Calculations_CreatedAt_State ON Calculations (
                        CreatedAt DESC,
                        State
                    ) WHERE State != {(int)CalculationState.Success};
                    """;

                command.ExecuteNonQuery();
            }
        }


        public UserDbModel GetOrAddUser(SqliteConnection connection, UserDbModel user)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = """
                    INSERT OR IGNORE INTO Users(Login) VALUES (@Login);
                    SELECT Id FROM Users WHERE Login = @Login;
                    """;

                command.Parameters.Add("@Login", SqliteType.Text).Value = user.Login;

                var userId = command.ExecuteScalar();
                if (userId == null)
                    throw new InvalidOperationException("Returned userId cannot be null");

                user.Id = (long)userId;
                return user;
            }
        }

        public CalculationDbModel AddCalculation(SqliteConnection connection, CalculationDbModel calculation)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = """
                    INSERT INTO Calculations(Id, Expression, CreatedAt, CreatedBy, UpdatedAt, State, CalcResult, ErrorCode, ErrorDetails, CancelledBy)
                    VALUES (@Id, @Expression, @CreatedAt, @CreatedBy, @UpdatedAt, @State, @CalcResult, @ErrorCode, @ErrorDetails, @CancelledBy)
                    """;

                long? errorCode = calculation.ErrorCode.HasValue ? (long)calculation.ErrorCode.Value : null;

                command.Parameters.Add("@Id", SqliteType.Blob).Value = calculation.Id;
                command.Parameters.Add("@Expression", SqliteType.Text).Value = calculation.Expression;
                command.Parameters.Add("@CreatedAt", SqliteType.Integer).Value = calculation.CreatedAt;
                command.Parameters.Add("@CreatedBy", SqliteType.Integer).Value = calculation.CreatedById;
                command.Parameters.Add("@UpdatedAt", SqliteType.Integer).Value = calculation.UpdatedAt;
                command.Parameters.Add("@State", SqliteType.Integer).Value = (long)calculation.State;
                command.Parameters.Add("@CalcResult", SqliteType.Real).Value = calculation.CalcResult ?? (object)DBNull.Value;
                command.Parameters.Add("@ErrorCode", SqliteType.Integer).Value = errorCode ?? (object)DBNull.Value;
                command.Parameters.Add("@ErrorDetails", SqliteType.Text).Value = calculation.ErrorDetails?.JsonDetails ?? (object)DBNull.Value;
                command.Parameters.Add("@CancelledBy", SqliteType.Integer).Value = calculation.CancelledById ?? (object)DBNull.Value;

                try
                {
                    command.ExecuteNonQuery();
                }
                catch (SqliteException e) when (e.SqliteErrorCode == 19)
                {
                    throw new DuplicatePrimaryKeyException($"Duplcate calculation id detected: {calculation.Id}", e);
                }

                return calculation;
            }
        }

        public bool TryUpdateCalculationStatus(SqliteConnection connection, ICaluclationStatusDbModelView calculationStatus)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = """
                    UPDATE Calculations
                    SET UpdatedAt = @UpdatedAt,
                        State = @State,
                        CalcResult = @CalcResult,
                        ErrorCode = @ErrorCode,
                        ErrorDetails = @ErrorDetails,
                        CancelledBy = @CancelledBy
                    WHERE Id = @Id
                    """;

                long? errorCode = calculationStatus.ErrorCode.HasValue ? (long)calculationStatus.ErrorCode.Value : null;

                command.Parameters.Add("@Id", SqliteType.Blob).Value = calculationStatus.Id;
                command.Parameters.Add("@UpdatedAt", SqliteType.Integer).Value = calculationStatus.UpdatedAt;
                command.Parameters.Add("@State", SqliteType.Integer).Value = (long)calculationStatus.State;
                command.Parameters.Add("@CalcResult", SqliteType.Real).Value = calculationStatus.CalcResult ?? (object)DBNull.Value;
                command.Parameters.Add("@ErrorCode", SqliteType.Integer).Value = errorCode ?? (object)DBNull.Value;
                command.Parameters.Add("@ErrorDetails", SqliteType.Text).Value = calculationStatus.ErrorDetails?.JsonDetails ?? (object)DBNull.Value;
                command.Parameters.Add("@CancelledBy", SqliteType.Integer).Value = calculationStatus.CancelledById ?? (object)DBNull.Value;

                return command.ExecuteNonQuery() == 1;
            }
        }


        private static CalculationDbModel ReadCalculation(SqliteDataReader reader)
        {
            var result = new CalculationDbModel()
            {
                Id = reader.GetGuid(0),
                Expression = reader.GetString(1),
                CreatedAt = reader.GetInt64(2),
                CreatedById = reader.GetInt64(3),
                UpdatedAt = reader.GetInt64(4),
                State = (CalculationState)reader.GetInt64(5),
                CalcResult = reader.IsDBNull(6) ? null : reader.GetDouble(6),
                ErrorCode = reader.IsDBNull(7) ? null : (CalculationErrorCode)reader.GetInt64(7),
                ErrorDetails = reader.IsDBNull(8) ? null : new CalculationErrorDetailsDbModel(reader.GetString(8)),
                CancelledById = reader.IsDBNull(9) ? null : reader.GetInt64(9)
            };

            result.CreatedBy = new UserDbModel()
            {
                Id = result.CreatedById,
                Login = reader.GetString(10)
            };

            if (result.CancelledById != null)
            {
                result.CancelledBy = new UserDbModel()
                {
                    Id = result.CancelledById.Value,
                    Login = reader.GetString(11)
                };
            }

            return result;
        }

        private static void FillGetCalculationsCommand(SqliteCommand command, CalculationFilters filters, PaginationParams pagination)
        {
            StringBuilder conditionBuilder = new StringBuilder();
            bool appendAnd = false;
            if (filters.Id != null)
            {
                if (appendAnd) 
                    conditionBuilder.Append(" AND ");
                conditionBuilder.Append("Calculations.Id = @Id");
                command.Parameters.Add("@Id", SqliteType.Blob).Value = filters.Id.Value;
                appendAnd = true;
            }
            if (filters.CreatedBy != null)
            {
                if (appendAnd)
                    conditionBuilder.Append(" AND ");
                conditionBuilder.Append("creator.Login = @CreatorLogin");
                command.Parameters.Add("@CreatorLogin", SqliteType.Text).Value = filters.CreatedBy;
                appendAnd = true;
            }
            if (filters.CreatedAtMin != null)
            {
                if (appendAnd)
                    conditionBuilder.Append(" AND ");
                conditionBuilder.Append("CreatedAt >= @CreatedAtMin");
                command.Parameters.Add("@CreatedAtMin", SqliteType.Integer).Value = CommonConversions.DateTimeToTimestamp(filters.CreatedAtMin.Value);
                appendAnd = true;
            }
            if (filters.CreatedAtMax != null)
            {
                if (appendAnd)
                    conditionBuilder.Append(" AND ");
                conditionBuilder.Append("CreatedAt < @CreatedAtMax");
                command.Parameters.Add("@CreatedAtMax", SqliteType.Integer).Value = CommonConversions.DateTimeToTimestamp(filters.CreatedAtMax.Value);
                appendAnd = true;
            }
            if (filters.UpdatedAtMin != null)
            {
                if (appendAnd)
                    conditionBuilder.Append(" AND ");
                conditionBuilder.Append("UpdatedAt >= @UpdatedAtMin");
                command.Parameters.Add("@UpdatedAtMin", SqliteType.Integer).Value = CommonConversions.DateTimeToTimestamp(filters.UpdatedAtMin.Value);
                appendAnd = true;
            }
            if (filters.UpdatedAtMax != null)
            {
                if (appendAnd)
                    conditionBuilder.Append(" AND ");
                conditionBuilder.Append("UpdatedAt < @UpdatedAtMax");
                command.Parameters.Add("@UpdatedAtMax", SqliteType.Integer).Value = CommonConversions.DateTimeToTimestamp(filters.UpdatedAtMax.Value);
                appendAnd = true;
            }
            if (filters.State != null)
            {
                if (appendAnd)
                    conditionBuilder.Append(" AND ");
                conditionBuilder.Append("State = @State");
                command.Parameters.Add("@State", SqliteType.Integer).Value = (long)filters.State.Value;
                appendAnd = true;
            }
            if (filters.Expression != null)
            {
                if (appendAnd)
                    conditionBuilder.Append(" AND ");
                conditionBuilder.Append("Expression LIKE @Expression");
                command.Parameters.Add("@Expression", SqliteType.Text).Value = "%" + filters.Expression + "%";
                appendAnd = true;
            }

            if (!appendAnd)
                conditionBuilder.Append("1 = 1");

            string limitOffset = "";
            if (pagination.Offset > 0 || pagination.Limit < uint.MaxValue)
            {
                limitOffset = $"LIMIT {pagination.Limit} OFFSET {pagination.Offset}";
            }

            command.CommandText = $"""
                SELECT Calculations.Id, Expression, CreatedAt, CreatedBy, UpdatedAt, State, CalcResult, ErrorCode, ErrorDetails, CancelledBy, creator.Login AS "CreatedByLogin", canceller.Login AS "CancelledByLogin"
                FROM Calculations 
                JOIN Users creator ON Calculations.CreatedBy = creator.Id
                LEFT JOIN Users canceller ON Calculations.CancelledBy = canceller.Id
                WHERE {conditionBuilder}
                ORDER BY CreatedAt DESC
                {limitOffset};
                """;

            if (pagination.IncludeCount)
            {
                command.CommandText += $"""

                    SELECT COUNT(Calculations.Id)
                    FROM Calculations 
                    JOIN Users creator ON Calculations.CreatedBy = creator.Id
                    WHERE {conditionBuilder};
                    """;
            }
        }
        public PaginatedResult<T> GetCalculationsList<T>(SqliteConnection connection, CalculationFilters filters, PaginationParams pagination, Func<CalculationDbModel, T> transformer)
        {
            using (var command = connection.CreateCommand())
            {
                FillGetCalculationsCommand(command, filters, pagination);

                List<T> result = new List<T>();
                uint? pagesCount = null;
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var dbModel = ReadCalculation(reader);
                        result.Add(transformer(dbModel));
                    }

                    if (pagination.IncludeCount)
                    {
                        if (!reader.NextResult() || !reader.Read())
                            throw new InvalidOperationException("Second result should conain the number of items");

                        pagesCount = (uint)reader.GetInt32(0);
                    }
                }

                return new PaginatedResult<T>(result, pagination.Offset, pagination.Limit, pagesCount);
            }
        }
        public PaginatedResult<CalculationDbModel> GetCalculationsList(SqliteConnection connection, CalculationFilters filters, PaginationParams pagination)
        {
            return GetCalculationsList(connection, filters, pagination, v => v);
        }


        public CalculationDbModel GetCalculationById(SqliteConnection connection, Guid id)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = """
                    SELECT Calculations.Id, Expression, CreatedAt, CreatedBy, UpdatedAt, State, CalcResult, ErrorCode, ErrorDetails, CancelledBy, creator.Login AS "CreatedByLogin", canceller.Login AS "CancelledByLogin"
                    FROM Calculations 
                    JOIN Users creator ON Calculations.CreatedBy = creator.Id
                    LEFT JOIN Users canceller ON Calculations.CancelledBy = canceller.Id
                    WHERE Calculations.Id = @Id
                    """;

                command.Parameters.Add("@Id", SqliteType.Blob).Value = id;

                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                        throw new EntityNotFoundException($"Entity for specified key not found. Key = {id}");

                    return ReadCalculation(reader);
                }
            }
        }

        public bool ContainsCalculation(SqliteConnection connection, Guid id)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = """
                    SELECT 1
                    FROM Calculations 
                    WHERE Id = @Id
                    """;

                command.Parameters.Add("@Id", SqliteType.Blob).Value = id;
                var result = command.ExecuteScalar();
                return result != null && (long)result == 1;
            }
        }
    }
}
