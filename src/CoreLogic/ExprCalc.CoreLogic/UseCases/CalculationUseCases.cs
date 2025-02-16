using ExprCalc.CoreLogic.Api.UseCases;
using ExprCalc.CoreLogic.Instrumentation;
using ExprCalc.Entities;
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
        ILogger<CalculationUseCases> logger,
        InstrumentationContainer instrumentation) : ICalculationUseCases
    {
        private readonly ILogger<CalculationUseCases> _logger = logger;
        private readonly CalculationUseCasesMetrics _metrics = instrumentation.CalculationUseCasesMetrics;
        private readonly ActivitySource _activitySource = instrumentation.ActivitySource;


        public Task<List<Calculation>> GetCalculationsListAsync(CancellationToken token)
        {
            _logger.LogTrace(nameof(GetCalculationsListAsync) + " started");
            _metrics.GetCalculationsList.AddCall();
            using var activity = _activitySource.StartActivity(nameof(CalculationUseCases) + "." + nameof(GetCalculationsListAsync));

            try
            {
                return Task.FromResult(new List<Calculation>()
                {
                    new Calculation() { Id = Guid.CreateVersion7(), Expression = "1 + 2", CreatedAt = DateTime.UtcNow },
                    new Calculation() { Id = Guid.CreateVersion7(), Expression = "1 * 2", CreatedAt = DateTime.UtcNow },
                    new Calculation() { Id = Guid.CreateVersion7(), Expression = "1 / 2", CreatedAt = DateTime.UtcNow },
                });
            }
            catch
            {
                _metrics.GetCalculationsList.AddFail();
                throw;
            }
        }

        public Task<Calculation> CreateCalculationAsync(Calculation calculation, CancellationToken token)
        {
            _logger.LogTrace(nameof(CreateCalculationAsync) + " started");
            _metrics.CreateCalculation.AddCall();
            using var activity = _activitySource.StartActivity(nameof(CalculationUseCases) + "." + nameof(CreateCalculationAsync));

            try
            {
                calculation.Id = Guid.CreateVersion7();
                calculation.CreatedAt = DateTime.UtcNow;

                return Task.FromResult(calculation);
            }
            catch
            {
                _metrics.CreateCalculation.AddFail();
                throw;
            }
        }
    }
}
