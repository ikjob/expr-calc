using ExprCalc.CoreLogic.Configuration;
using ExprCalc.CoreLogic.Instrumentation;
using ExprCalc.CoreLogic.Resources.CalculationsRegistry;
using ExprCalc.CoreLogic.Resources.ExpressionCalculation;
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
        private readonly IExpressionCalculator _calculator;

        private readonly int _processorCount;
        private readonly ActivitySource _activitySource;
        private readonly CalculationsProcessingMetrics _metrics;
        private readonly ILogger<CalculationsProcessingService> _logger;

        public CalculationsProcessingService(
            IScheduledCalculationsRegistry calculationsRegistry,
            IExpressionCalculator calculator,
            IOptions<CoreLogicConfig> config,
            ILogger<CalculationsProcessingService> logger,
            InstrumentationContainer instrumentation)
        {
            _calculationsRegistry = calculationsRegistry;
            _calculator = calculator;

            _processorCount = config.Value.CalculationProcessorsCount;
            if (_processorCount <= 0)
                _processorCount = Environment.ProcessorCount;

            _activitySource = instrumentation.ActivitySource;
            _metrics = instrumentation.CalculationsProcessingMetrics;
            _logger = logger;

            _metrics.SetInitialValues(_processorCount);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Calculation processing service is starting. Number of processors = {processorsCount}", _processorCount);

            var processors = new Task[_processorCount];
            for (int i = 0; i < _processorCount; i++)
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


        /// <summary>
        /// Core processing loop for <see cref="ExprCalc.Entities.Calculation"/> taken from <see cref="IScheduledCalculationsRegistry"/>
        /// </summary>
        private async Task WorkerLoop(int workerIndex, CancellationToken stoppingToken)
        {
            await Task.Yield();
            _logger.LogDebug("Calculations processing background worker #{workerIndex} has started", workerIndex);


            while (!stoppingToken.IsCancellationRequested)
            {
                using var newCalculation = await _calculationsRegistry.TakeNextForProcessing(stoppingToken);
                _metrics.ProcessedTotal.Add(1);
                
                using var activity = _activitySource.StartActivity(nameof(CalculationsProcessingService) + ".NewCalculation");
                _logger.LogTrace("New expression taken for processing. Id = {id}, Expression = {expression}", newCalculation.Calculation.Id, newCalculation.Calculation.Expression);
                var stopwatch = Stopwatch.StartNew();

                try
                {
                    var finalState = await _calculator.Calculate(newCalculation.Calculation, newCalculation.Token, stoppingToken);

                    if (finalState.IsSuccess(out _))
                    {
                        _metrics.ProcessedSuccessfullyCount.Add(1);
                    }
                    else if (finalState.IsCancelled(out _))
                    {
                        _logger.LogDebug("Calculation was cancelled by the user. Id = {id}", newCalculation.Calculation.Id);
                        _metrics.ProcessedWasCancelledCount.Add(1);
                    }
                    else if (finalState.IsFailed(out var failedStatus))
                    {
                        _logger.LogDebug("Calculation was failed. Id = {id}. ErrorCode = {errorCode}", newCalculation.Calculation.Id, failedStatus.ErrorDetails.ErrorCode);
                        _metrics.ProcessedWithFailureCount.Add(1);
                    }
                    else if (newCalculation.Token.IsCancellationRequested)
                    {
                        _logger.LogDebug("Calculation was cancelled by the system. Id = {id}", newCalculation.Calculation.Id);
                        _metrics.ProcessedWasCancelledCount.Add(1);
                    }
                    else
                    {
                        _logger.LogWarning("Unexpected final state of calculation: {state}", finalState.State);
                        _metrics.ProcessedWithFailureCount.Add(1);
                    }
                }
                catch (OperationCanceledException) when (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogDebug("Calculation was cancelled. Id = {id}", newCalculation.Calculation.Id);
                    _metrics.ProcessedWasCancelledCount.Add(1);
                }

                _metrics.ProcessingTimeCounter.Record(stopwatch.ElapsedMilliseconds);
            }
        }
    }
}
