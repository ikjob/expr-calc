using ExprCalc.CoreLogic.Configuration;
using ExprCalc.CoreLogic.Instrumentation;
using ExprCalc.CoreLogic.Resources.CalculationsRegistry;
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
    /// Removes old calculations from storage
    /// </summary>
    internal class StorageCleanupJob
    {
        private readonly IScheduledCalculationsRegistry _calculationRegistry;
        private readonly ICalculationRepository _calculationsRepository;

        private readonly TimeSpan _expirationPeriod;

        private readonly ActivitySource _activitySource;
        private readonly ILogger _logger;

        public StorageCleanupJob(
            IScheduledCalculationsRegistry calculationRegistry,
            ICalculationRepository calculationRepository,
            IOptions<CoreLogicConfig> config,
            ILogger logger,
            InstrumentationContainer instrumentation)
        {
            _calculationRegistry = calculationRegistry;
            _calculationsRepository = calculationRepository;

            _expirationPeriod = config.Value.StorageCleanupExpiration;

            _activitySource = instrumentation.ActivitySource;
            _logger = logger;
        }

        public async Task ExecuteAsync(CancellationToken token)
        {
            DateTime deleteBefore = DateTime.UtcNow.Subtract(_expirationPeriod);
            _logger.LogInformation("Cleanup procedure started");
            using var activity = _activitySource.StartActivity(nameof(StorageCleanupJob) + ".Cleanup");

            Stopwatch sw = Stopwatch.StartNew();
            // Remove from database first
            int deletedCount = await _calculationsRepository.DeleteCalculationsAsync(deleteBefore, token);

            // Remove from registry
            List<Guid> guidsToRemove = new List<Guid>();
            foreach (var item in _calculationRegistry.Enumerate(withCancelled: false))
            {
                if (item.CreatedAt < deleteBefore)
                {
                    guidsToRemove.Add(item.Id);
                }
            }

            foreach (var id in guidsToRemove)
            {
                if (_calculationRegistry.TryCancel(id, Entities.User.System, out _))
                {
                    // Due to possibility of race conditions it is important to remove explicitly from storage
                    await _calculationsRepository.DeleteCalculationByIdAsync(id, token);
                }
            }

            _logger.LogInformation("Cleanup procedure removed {num} calculations. Procedure took {time}ms", deletedCount, sw.ElapsedMilliseconds);
        }
    }
}
