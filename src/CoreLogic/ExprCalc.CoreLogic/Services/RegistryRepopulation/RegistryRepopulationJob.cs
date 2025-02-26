using ExprCalc.CoreLogic.Configuration;
using ExprCalc.CoreLogic.Instrumentation;
using ExprCalc.CoreLogic.Resources.CalculationsRegistry;
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

namespace ExprCalc.CoreLogic.Services.RegistryRepopulation
{
    /// <summary>
    /// Job that runs at the startup and repopulates <see cref="IScheduledCalculationsRegistry"/> from storage
    /// </summary>
    internal class RegistryRepopulationJob : BackgroundService
    {
        private readonly IScheduledCalculationsRegistry _calculationsRegistry;
        private readonly ICalculationRepository _calculationsRepository;

        private readonly DateTime _startTime;
        private readonly int _singleBatchSize;

        private readonly ActivitySource _activitySource;
        private readonly ILogger<RegistryRepopulationJob> _logger;

        public RegistryRepopulationJob(
            IScheduledCalculationsRegistry calculationsRegistry,
            ICalculationRepository calculationRepository,
            IOptions<CoreLogicConfig> config,
            ILogger<RegistryRepopulationJob> logger,
            InstrumentationContainer instrumentation)
        {
            _startTime = DateTime.UtcNow;

            _calculationsRegistry = calculationsRegistry;
            _calculationsRepository = calculationRepository;

            _singleBatchSize = Math.Max(10, config.Value.MaxRegisteredCalculationsCount / 2);

            _activitySource = instrumentation.ActivitySource;
            _logger = logger;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Registry repopulation started");
            await Task.Yield();



            _logger.LogInformation("Registry repopulation finished");
        }
    }
}
