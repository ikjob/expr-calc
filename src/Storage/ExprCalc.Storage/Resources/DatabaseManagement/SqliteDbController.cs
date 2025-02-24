using DotNext.Threading;
using ExprCalc.Storage.Configuration;
using ExprCalc.Storage.Resources.SqliteQueries;
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
    internal class SqliteDbController : IDatabaseController, IDisposable, IAsyncDisposable
    {
        public const string DatabaseFileName = "expr_calc.sqlite";

        private readonly string _databaseDirectory;
        private readonly string _writeConnectionString;
        private readonly string _readConnectionString;

        private volatile SqliteConnection? _writeConnection;
        private readonly AsyncExclusiveLock _writerLock;
        private readonly AsyncReaderWriterLock _queryRwLock;
        
        private readonly ISqlDbQueryProvider _queryProvider;

        private readonly ILogger<SqliteDbController> _logger;

        private volatile bool _disposed;

        public SqliteDbController(
            ISqlDbQueryProvider queryProvider, 
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
  

            _queryProvider = queryProvider;
            _logger = logger;

            _disposed = false;
        }
        [ActivatorUtilitiesConstructor]
        public SqliteDbController(
            ISqlDbQueryProvider queryProvider,
            IOptions<StorageConfig> config,
            ILogger<SqliteDbController> logger)
            : this(queryProvider, config.Value.DatabaseDirectory, logger)
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
                await _writeConnection.OpenAsync(token);

                await _queryProvider.InitializeDbIfNeeded(_writeConnection, token);
            }
        }

        public Task Init(CancellationToken token)
        {
            return EnsureInitialized(token);
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
