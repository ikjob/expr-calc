using DotNext.Threading;
using ExprCalc.Entities;
using ExprCalc.Entities.MetadataParams;
using ExprCalc.Storage.Configuration;
using ExprCalc.Storage.Resources.SqliteQueries;
using ExprCalc.Storage.Resources.SqliteQueries.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.Storage.Resources.DatabaseManagement
{
    /// <summary>
    /// Manages single SQLite database file
    /// </summary>
    internal class SqliteDbController : IDatabaseController, IDisposable, IAsyncDisposable
    {
        public const string DatabaseFileName = "expr_calc.sqlite";

        private readonly string _databaseDirectory;
        private readonly string _writeConnectionString;
        private readonly string _readConnectionString;

        private volatile SqliteConnection? _writeConnection;
        /// <summary>
        /// Mutex that protects access to <see cref="_writeConnection"/>
        /// </summary>
        private readonly AsyncExclusiveLock _writerLock;
        /// <summary>
        /// RwLock that used to prevent query execution during initialization or disposing.
        /// Write lock acquired in initialization and disposing phases. Read lock acquired in all queries
        /// </summary>
        private readonly AsyncReaderWriterLock _queryRwLock;

        private readonly ISqlDbInitializationQueryProvider _initializationQueryProvider;
        private readonly ISqlDbCalculationsQueryProvider _calculationsQueryProvider;
        
        private readonly ILogger<SqliteDbController> _logger;

        private volatile bool _disposed;

        public SqliteDbController(
            ISqlDbInitializationQueryProvider initializationQueryProvider,
            ISqlDbCalculationsQueryProvider calculationsQueryProvider, 
            string databaseDirectory,
            ILogger<SqliteDbController> logger)
        {
            // Will throw exception on invalid path
            _databaseDirectory = Path.GetFullPath(databaseDirectory);

            var conStrings = GetConnectionStrings(_databaseDirectory);
            _writeConnectionString = conStrings.writeConString;
            _readConnectionString = conStrings.readConString;

            _writeConnection = null;
            _writerLock = new AsyncExclusiveLock();
            _queryRwLock = new AsyncReaderWriterLock();

            _initializationQueryProvider = initializationQueryProvider;
            _calculationsQueryProvider = calculationsQueryProvider;
            _logger = logger;

            _disposed = false;
        }
        [ActivatorUtilitiesConstructor]
        public SqliteDbController(
            ISqlDbInitializationQueryProvider initializationQueryProvider,
            ISqlDbCalculationsQueryProvider calculationsQueryProvider,
            IOptions<StorageConfig> config,
            ILogger<SqliteDbController> logger)
            : this(initializationQueryProvider, calculationsQueryProvider, config.Value.DatabaseDirectory, logger)
        {
        }

        private static (string writeConString, string readConString) GetConnectionStrings(string databaseDirectory)
        {
            var conStringBuilder = new SqliteConnectionStringBuilder();

            conStringBuilder.DataSource = Path.Combine(databaseDirectory, DatabaseFileName);
            conStringBuilder.Mode = SqliteOpenMode.ReadWriteCreate;
            conStringBuilder.Pooling = false;

            string writeConString = conStringBuilder.ToString();

            conStringBuilder.Mode = SqliteOpenMode.ReadOnly;
            conStringBuilder.Pooling = true;

            string readConString = conStringBuilder.ToString();

            return (writeConString, readConString);
        }


        private async Task EnsureInitialized(CancellationToken token)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(SqliteDbController));

            if (_writeConnection != null)
                return;

            using (await _queryRwLock.AcquireWriteLockAsync(token))
            {
                if (_disposed)
                    throw new ObjectDisposedException(nameof(SqliteDbController));

                if (_writeConnection != null)
                    return;

                if (!Directory.Exists(_databaseDirectory))
                    Directory.CreateDirectory(_databaseDirectory);

                _writeConnection = new SqliteConnection(_writeConnectionString);
                _writeConnection.Open();

                _initializationQueryProvider.InitializeDbIfNeeded(_writeConnection);
            }
        }

        public Task Init(CancellationToken token)
        {
            return EnsureInitialized(token);
        }


        public async Task<Calculation> AddCalculationAsync(Calculation calculation, CancellationToken token)
        {
            await EnsureInitialized(token);

            using (await _queryRwLock.AcquireReadLockAsync(token))
            using (await _writerLock.AcquireLockAsync(token))
            {
                if (_disposed || _writeConnection == null)
                    throw new ObjectDisposedException(nameof(SqliteDbController));

                
                using (var transaction = _writeConnection.BeginTransaction())
                {
                    var dbModel = CalculationDbModel.FromEntity(calculation);

                    dbModel.CreatedBy = _calculationsQueryProvider.GetOrAddUser(
                        _writeConnection,
                        dbModel.CreatedBy ?? throw new InvalidOperationException("CreatedBy should be set by converter"));
                    dbModel.CreatedById = dbModel.CreatedBy.Id;

                    if (dbModel.CancelledBy != null)
                    {
                        dbModel.CancelledBy = _calculationsQueryProvider.GetOrAddUser(
                            _writeConnection,
                            dbModel.CancelledBy);
                        dbModel.CancelledById = dbModel.CancelledBy.Id;
                    }

                    dbModel = _calculationsQueryProvider.AddCalculation(_writeConnection, dbModel);

                    transaction.Commit();
                }
            }

            return calculation;
        }

        public async Task<bool> TryUpdateCalculationStatusAsync(CalculationStatusUpdate calculationStatus, CancellationToken token)
        {
            await EnsureInitialized(token);

            bool result = false;

            using (await _queryRwLock.AcquireReadLockAsync(token))
            using (await _writerLock.AcquireLockAsync(token))
            {
                if (_disposed || _writeConnection == null)
                    throw new ObjectDisposedException(nameof(SqliteDbController));


                using (var transaction = _writeConnection.BeginTransaction())
                {
                    var dbModel = CalculationDbModel.FromStatusUpdateEntity(calculationStatus);

                    if (dbModel.CancelledBy != null)
                    {
                        dbModel.CancelledBy = _calculationsQueryProvider.GetOrAddUser(
                            _writeConnection,
                            dbModel.CancelledBy);
                        dbModel.CancelledById = dbModel.CancelledBy.Id;
                    }

                    result = _calculationsQueryProvider.TryUpdateCalculationStatus(_writeConnection, dbModel);

                    transaction.Commit();
                }
            }


            return result;
        }

        public async Task<PaginatedResult<Calculation>> GetCalculationsListAsync(CalculationFilters filters, PaginationParams pagination, CancellationToken token)
        {
            await EnsureInitialized(token);

            using (await _queryRwLock.AcquireReadLockAsync(token))
            {
                if (_disposed)
                    throw new ObjectDisposedException(nameof(SqliteDbController));

                using (var connection = new SqliteConnection(_readConnectionString))
                {
                    connection.Open();
                    return _calculationsQueryProvider.GetCalculationsList(connection, filters, pagination, v => v.IntoEntity());
                }
            }
        }

        public async Task<Calculation> GetCalculationByIdAsync(Guid id, CancellationToken token)
        {
            await EnsureInitialized(token);

            using (await _queryRwLock.AcquireReadLockAsync(token))
            {
                if (_disposed)
                    throw new ObjectDisposedException(nameof(SqliteDbController));

                using (var connection = new SqliteConnection(_readConnectionString))
                {
                    connection.Open();
                    return _calculationsQueryProvider.GetCalculationById(connection, id).IntoEntity();
                }
            }
        }

        public async Task<bool> ContainsCalculationAsync(Guid id, CancellationToken token)
        {
            await EnsureInitialized(token);

            using (await _queryRwLock.AcquireReadLockAsync(token))
            {
                if (_disposed)
                    throw new ObjectDisposedException(nameof(SqliteDbController));

                using (var connection = new SqliteConnection(_readConnectionString))
                {
                    connection.Open();
                    return _calculationsQueryProvider.ContainsCalculation(connection, id);
                }
            }
        }



        protected virtual void Dispose(bool isUserCall)
        {
            if (_disposed)
                return;

            if (isUserCall)
            {
                _disposed = true;

                using (_queryRwLock.AcquireWriteLock())
                {
                    var writeConnection = _writeConnection;
                    _writeConnection = null;
                    writeConnection?.Close();

                    SqliteConnection.ClearPool(new SqliteConnection(_readConnectionString));
                }
            }
        }

        protected virtual async ValueTask DisposeAsyncCore()
        {
            if (_disposed)
                return;

            _disposed = true;

            using (await _queryRwLock.AcquireWriteLockAsync())
            {
                var writeConnection = _writeConnection;
                _writeConnection = null;
                writeConnection?.Close();

                SqliteConnection.ClearPool(new SqliteConnection(_readConnectionString));
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore().ConfigureAwait(false);
            GC.SuppressFinalize(this);
        }
    }
}
