using ExprCalc.CoreLogic.Configuration;
using ExprCalc.CoreLogic.Instrumentation;
using ExprCalc.CoreLogic.Resources.CalculationsRegistry;
using ExprCalc.CoreLogic.Services.RegistryRepopulation;
using ExprCalc.Storage.Api.Repositories;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.CoreLogic.Services.StorageManagement
{
    /// <summary>
    /// Manages storage: 
    /// 1. Repopulate registry from storage on startup
    /// 2. Periodically removes old calculations from storage
    /// </summary>
    internal class StorageManagementService : BackgroundService
    {
        private readonly RegistryRepopulationJob _registryRepopulationJob;
        private readonly StorageCleanupJob _storageCleanupJob;

        private readonly TimeSpan _cleanupPeriod;

        private readonly ActivitySource _activitySource;
        private readonly ILogger<StorageManagementService> _logger;

        public StorageManagementService(
            ICalculationRepository calculationRepository,
            IScheduledCalculationsRegistry calculationRegistry,
            IOptions<CoreLogicConfig> config,
            ILogger<StorageManagementService> logger,
            InstrumentationContainer instrumentation)
        {
            _registryRepopulationJob = new RegistryRepopulationJob(calculationRegistry, calculationRepository, config, logger, instrumentation);
            _storageCleanupJob = new StorageCleanupJob(calculationRegistry, calculationRepository, config, logger, instrumentation);

            _cleanupPeriod = config.Value.StorageCleanupPeriod;

            _activitySource = instrumentation.ActivitySource;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Delay first run slightly
            await Task.Delay(TimeSpan.FromMilliseconds(250), stoppingToken).ConfigureAwait(false);

            if (_cleanupPeriod > TimeSpan.Zero)
            {
                // Storage cleanup enabled => run initial cleanup
                await _storageCleanupJob.ExecuteAsync(stoppingToken);
            }

            // Run repopulation after the first cleanup finished
            await _registryRepopulationJob.ExecuteAsync(stoppingToken);


            if (_cleanupPeriod <= TimeSpan.Zero)
            {
                _logger.LogInformation("Storage cleanup was disabled");
                return;
            }

            // Run cleanup periodically
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(_cleanupPeriod, stoppingToken);
                await _storageCleanupJob.ExecuteAsync(stoppingToken);
            }
        }
    }
}
