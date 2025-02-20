using ExprCalc.Entities;
using ExprCalc.Storage.Api.Repositories;
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
    internal class StatusUpdaterInStorage(ICalculationRepository storageRepo) : IExternalCalculationStatusUpdater
    {
        private readonly ICalculationRepository _storageRepo = storageRepo;

        public Task UpdateStatus(Calculation calculation, CancellationToken cancellationToken)
        {
            return _storageRepo.UpdateCalculationStatusAsync(new CalculationStatusUpdate(calculation.Id, calculation.UpdatedAt, calculation.Status), cancellationToken);
        }
    }
}
