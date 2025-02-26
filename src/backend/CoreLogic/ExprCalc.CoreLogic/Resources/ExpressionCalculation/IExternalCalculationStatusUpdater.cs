using ExprCalc.Entities;
using ExprCalc.Storage.Api.Exceptions;
using ExprCalc.Storage.Api.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.CoreLogic.Resources.ExpressionCalculation
{
    /// <summary>
    /// Provides a way to inject the status update logic inside external systems into <see cref="ExpressionCalculator"/>
    /// </summary>
    internal interface IExternalCalculationStatusUpdater
    {
        Task UpdateStatus(Calculation calculation, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Implements <see cref="IExternalCalculationStatusUpdater"/> and performs status update in storage
    /// </summary>
    /// <param name="storageRepo">Storage repositry to update the status</param>
    internal class StatusUpdaterInStorage(
        ICalculationRepository storageRepo, 
        ILogger<StatusUpdaterInStorage> logger) : IExternalCalculationStatusUpdater
    {
        private readonly ICalculationRepository _storageRepo = storageRepo;
        private readonly ILogger<StatusUpdaterInStorage> _logger = logger;

        public async Task UpdateStatus(Calculation calculation, CancellationToken cancellationToken)
        {
            try
            {
                await _storageRepo.UpdateCalculationStatusAsync(
                    new CalculationStatusUpdate(
                        calculation.Id, 
                        calculation.UpdatedAt, 
                        calculation.Status), 
                    cancellationToken);
            }
            catch (StorageEntityNotFoundException ex)
            {
                _logger.LogWarning(ex, "Can't update calculation status in storage, because calculation was not found");
                // It is possible that calculation was removed by the old records cleaning procedure,
                // so just log and continue execution
            }
        }
    }
}
