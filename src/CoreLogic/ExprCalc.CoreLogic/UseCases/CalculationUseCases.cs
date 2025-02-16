using ExprCalc.CoreLogic.Api.UseCases;
using ExprCalc.CoreLogic.Helpers;
using ExprCalc.CoreLogic.Instrumentation;
using ExprCalc.Entities;
using ExprCalc.Storage.Api.Exceptions;
using ExprCalc.Storage.Api.Repositories;
using Microsoft.Extensions.Logging;
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
        ILogger<CalculationUseCases> logger,
        InstrumentationContainer instrumentation) : ICalculationUseCases
    {
        private readonly ICalculationRepository _calculationRepository = calculationRepository;

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

        public async Task<Calculation> CreateCalculationAsync(Calculation calculation, CancellationToken token)
        {
            _logger.LogTrace(nameof(CreateCalculationAsync) + " started");
            _metrics.CreateCalculation.AddCall();
            using var activity = _activitySource.StartActivity(nameof(CalculationUseCases) + "." + nameof(CreateCalculationAsync));

            try
            {
                calculation.Initialize(Guid.CreateVersion7());
                return await _calculationRepository.CreateCalculationAsync(calculation, token);
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
    }
}
