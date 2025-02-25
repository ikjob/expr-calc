using ExprCalc.Storage.Resources.DatabaseManagement;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.Storage.Services.DatabaseMonitoring
{
    internal class DatabaseMonitoringService : BackgroundService
    {
        private readonly IDatabaseController _databaseController;

        private readonly ILogger<DatabaseMonitoringService> _logger;

        public DatabaseMonitoringService(
            IDatabaseController databaseController,
            ILogger<DatabaseMonitoringService> logger)
        {
            _databaseController = databaseController;
            _logger = logger;
        }


        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            // Run db startup
            await _databaseController.Init(cancellationToken);

            await base.StartAsync(cancellationToken);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }
    }
}
