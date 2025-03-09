using ExprCalc.CoreLogic.Configuration;
using ExprCalc.CoreLogic.Instrumentation;
using ExprCalc.CoreLogic.Resources.CalculationsRegistry;
using ExprCalc.Entities.MetadataParams;
using ExprCalc.Storage.Api.Repositories;
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
    /// Job that runs at the startup and repopulates <see cref="IScheduledCalculationsRegistry"/> from storage
    /// </summary>
    internal class RegistryRepopulationJob
    {
        private readonly IScheduledCalculationsRegistry _calculationsRegistry;
        private readonly ICalculationRepository _calculationsRepository;

        private readonly DateTime _startTime;
        private readonly int _singleBatchSize;
        private readonly TimeSpan _repopulationDelay;

        private readonly ActivitySource _activitySource;
        private readonly ILogger _logger;

        public RegistryRepopulationJob(
            IScheduledCalculationsRegistry calculationsRegistry,
            ICalculationRepository calculationRepository,
            IOptions<CoreLogicConfig> config,
            ILogger logger,
            InstrumentationContainer instrumentation)
        {
            _startTime = DateTime.UtcNow;

            _calculationsRegistry = calculationsRegistry;
            _calculationsRepository = calculationRepository;

            _singleBatchSize = config.Value.RegistryRepopulationBatch;
            _repopulationDelay = config.Value.RegistryRepopulationDelay;

            _activitySource = instrumentation.ActivitySource;
            _logger = logger;
        }


        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Registry repopulation started");
            using var activity = _activitySource.StartActivity(nameof(RegistryRepopulationJob) + ".Repopulation");

            int resetStatesCount = await _calculationsRepository.ResetNonFinalStateToPendingAsync(_startTime, _startTime, stoppingToken);
            _logger.LogInformation("Repopulation procedure reset statuses in storage for {num} calculations", resetStatesCount);


            var filter = new CalculationFilters() { State = Entities.Enums.CalculationState.Pending, CreatedAtMax = _startTime };
            var pagination = new PaginationParams(0, (uint)_singleBatchSize);
            bool hasProgress = true;
            int lastBatchItemsCount = int.MaxValue;

            while (hasProgress && lastBatchItemsCount >= pagination.Limit)
            {
                stoppingToken.ThrowIfCancellationRequested();

                var batch = await _calculationsRepository.GetCalculationsListAsync(filter, pagination, stoppingToken);
                lastBatchItemsCount = batch.Items.Count;
                if (batch.Items.Count == 0)
                    break;

                hasProgress = false;
                DateTime lastCreatedAt = DateTime.MaxValue;
                foreach (var calculation in batch.Items)
                {
                    Debug.Assert(calculation.Status.State == Entities.Enums.CalculationState.Pending);
                    Debug.Assert(calculation.CreatedAt <= lastCreatedAt);

                    if (!_calculationsRegistry.Contains(calculation.Id))
                    {
                        while (!_calculationsRegistry.TryAdd(calculation, TimeSpan.Zero))
                        {
                            _logger.LogInformation("Registry overloaded. Delay repopulation processs for {delay}", _repopulationDelay);
                            await Task.Delay(_repopulationDelay, stoppingToken);
                        }
                        hasProgress = true;
                    }

                    lastCreatedAt = calculation.CreatedAt;
                }

                _logger.LogDebug("Repopulation processed batch: {size}", batch.Items.Count);
                // Time is not unique, so add 1 millsecond to capture all records. 
                // This will lead to duplicate records but it is not a big problem
                filter = filter with { CreatedAtMax = lastCreatedAt.AddMilliseconds(1) };
            }


            _logger.LogInformation("Registry repopulation finished");
        }
    }
}
