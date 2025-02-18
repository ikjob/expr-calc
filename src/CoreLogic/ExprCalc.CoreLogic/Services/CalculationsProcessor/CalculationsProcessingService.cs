using ExprCalc.CoreLogic.Configuration;
using ExprCalc.CoreLogic.Instrumentation;
using ExprCalc.CoreLogic.Resources.CalculationsRegistry;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.CoreLogic.Services.CalculationsProcessor
{
    /// <summary>
    /// Background service that runs calculations
    /// </summary>
    internal class CalculationsProcessingService : BackgroundService
    {
        private readonly IScheduledCalculationsRegistry _calculationsRegistry;

        private readonly CoreLogicConfig _config;
        private readonly ActivitySource _activitySource;
        private readonly ILogger<CalculationsProcessingService> _logger;

        public CalculationsProcessingService(
            IScheduledCalculationsRegistry calculationsRegistry,
            IOptions<CoreLogicConfig> config,
            ILogger<CalculationsProcessingService> logger,
            InstrumentationContainer instrumentation)
        {
            _calculationsRegistry = calculationsRegistry;

            _config = config.Value;
            _logger = logger;
            _activitySource = instrumentation.ActivitySource;

            if (_config.CalculationProcessorsCount == 0 || _config.CalculationProcessorsCount < -1)
                throw new ArgumentOutOfRangeException(nameof(config), "Number of processors in config cannot be zero or negative (only -1 has special meaning)");

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            int processorsCount = _config.CalculationProcessorsCount;
            if (processorsCount <= 0)
                processorsCount = Environment.ProcessorCount;

            _logger.LogInformation("Calculation processing service is starting. Number of processors = {processorsCount}", _config.CalculationProcessorsCount);

            var processors = new Task[processorsCount];
            for (int i = 0; i < processorsCount; i++)
                processors[i] = WorkerLoop(i, stoppingToken);

            var completed = await Task.WhenAny(processors);
            if (completed.IsFaulted)
            {
                _logger.LogError(completed.Exception, "One of the background calculations workers has completed with exception");
                await completed; // Task is faulted => propagate its exception
            }

            if (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogError("One of the background calculations workers has stopped unexpectedly");
                throw new BackgroundWorkerStoppedUnexpectedlyException("One of the background calculations workers has stopped unexpectedly"); // Task stopped unexpectedly
            }

            // Wait for completion of all other tasks
            await Task.WhenAll(processors);
        }



        private async Task WorkerLoop(int workerIndex, CancellationToken stoppingToken)
        {
            await Task.Yield();
            _logger.LogDebug("Calculations processing background worker #{workerIndex} has started", workerIndex);


            while (!stoppingToken.IsCancellationRequested)
            {
                using var newCalculation = await _calculationsRegistry.TakeNextForProcessing(stoppingToken);
                using var activity = _activitySource.StartActivity(nameof(CalculationsProcessingService) + ".NewCalculation");
                using var linkedCancellationSource = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, newCalculation.Token);
                _logger.LogTrace("New expression taken for processing. Id = {id}, Expression = {expression}", newCalculation.Calculation.Id, newCalculation.Calculation.Expression);

                // Emulate calculation
                await Task.Delay(1000, linkedCancellationSource.Token);
                
            }
        }
    }
}
