using ExprCalc.CoreLogic.Api.Exceptions;
using ExprCalc.CoreLogic.Api.UseCases;
using ExprCalc.CoreLogic.Configuration;
using ExprCalc.CoreLogic.Helpers;
using ExprCalc.CoreLogic.Instrumentation;
using ExprCalc.CoreLogic.Resources.CalculationsRegistry;
using ExprCalc.Entities;
using ExprCalc.Storage.Api.Exceptions;
using ExprCalc.Storage.Api.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.CoreLogic.UseCases
{
    internal class CalculationUseCases(
        ICalculationRepository calculationRepository,
        IScheduledCalculationsRegistry calculationsRegistry,
        IOptions<CoreLogicConfig> config,
        ILogger<CalculationUseCases> logger,
        InstrumentationContainer instrumentation) : ICalculationUseCases
    {
        private readonly ICalculationRepository _calculationRepository = calculationRepository;
        private readonly IScheduledCalculationsRegistry _calculationsRegistry = calculationsRegistry;

        private readonly CoreLogicConfig _config = config.Value;
        private readonly ILogger<CalculationUseCases> _logger = logger;
        private readonly CalculationUseCasesMetrics _metrics = instrumentation.CalculationUseCasesMetrics;
        private readonly ActivitySource _activitySource = instrumentation.ActivitySource;


        public async Task<List<Calculation>> GetCalculationsListAsync(CancellationToken token)
        {
            _logger.LogTrace(nameof(GetCalculationsListAsync) + " started");
            _metrics.GetCalculationsList.AddCall();
            using var activity = _activitySource.StartActivity(nameof(CalculationUseCases) + "." + nameof(GetCalculationsListAsync));

            try
            {
                return await _calculationRepository.GetCalculationsListAsync(token);
            }
            catch (Exception exc)
            {
                _metrics.GetCalculationsList.AddFail();
                activity?.SetStatus(ActivityStatusCode.Error, "Excpetion: " + exc.Message);

                if (exc is StorageException storageExc && storageExc.TryTranslateStorageException(out var translatedException))
                    throw translatedException;

                throw;
            }
        }


        private TimeSpan GenerateRandomDelay()
        {
            long millisecondsMax = (long)_config.MaxCalculationAvailabilityDelay.TotalMilliseconds;
            long millisecondsMin = (long)_config.MinCalculationAvailabilityDelay.TotalMilliseconds;
            return TimeSpan.FromMilliseconds(Random.Shared.NextInt64(millisecondsMin, millisecondsMax));
        }
        public async Task<Calculation> CreateCalculationAsync(Calculation calculation, CancellationToken token)
        {
            if (!calculation.Status.IsPending())
                throw new ArgumentException("Only calculations in Pending status allowed", nameof(calculation));

            _logger.LogTrace(nameof(CreateCalculationAsync) + " started");
            _metrics.CreateCalculation.AddCall();
            using var activity = _activitySource.StartActivity(nameof(CalculationUseCases) + "." + nameof(CreateCalculationAsync));

            try
            {
                using (var slot = _calculationsRegistry.TryReserveSlot(calculation))
                {
                    if (!slot.IsAvailable)
                        throw new TooManyPendingCalculationsException("Too many pending calculations in registry");

                    var createdCalculation = await _calculationRepository.AddCalculationAsync(calculation, token);
                    slot.Fill(createdCalculation, delayBeforeExecution: GenerateRandomDelay());
                    return createdCalculation;
                }
            }
            catch (Exception exc)
            {
                _metrics.CreateCalculation.AddFail();
                activity?.SetStatus(ActivityStatusCode.Error, "Excpetion: " + exc.Message);

                if (exc is StorageException storageExc && storageExc.TryTranslateStorageException(out var translatedException))
                    throw translatedException;

                throw;
            }
        }

        public async Task<CalculationStatusUpdate> CancelCalculationAsync(Guid id, User cancelledBy, CancellationToken token)
        {
            _logger.LogTrace(nameof(CancelCalculationAsync) + " started");
            _metrics.CancelCalculation.AddCall();
            using var activity = _activitySource.StartActivity(nameof(CalculationUseCases) + "." + nameof(CancelCalculationAsync));

            try
            {
                if (!_calculationsRegistry.TryCancel(id, cancelledBy, out var statusUpdate))
                    throw new EntityNotFoundException($"Calculation for sepcified id = {id} is not exists or not processed now");

                await _calculationRepository.UpdateCalculationStatusAsync(statusUpdate.Value, token);
                return statusUpdate.Value;
            }
            catch (Exception exc)
            {
                _metrics.CancelCalculation.AddFail();
                activity?.SetStatus(ActivityStatusCode.Error, "Excpetion: " + exc.Message);

                if (exc is StorageException storageExc && storageExc.TryTranslateStorageException(out var translatedException))
                    throw translatedException;

                throw;
            }
        }
    }
}
