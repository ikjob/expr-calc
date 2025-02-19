using ExprCalc.Entities;
using ExprCalc.Storage.Api.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.CoreLogic.Resources.ExpressionCalculation
{
    internal interface IExternalCalculationStatusUpdater
    {
        Task UpdateStatus(Calculation calculation, CancellationToken cancellationToken);
    }


    internal class StatusUpdaterInStorage(ICalculationRepository storageRepo) : IExternalCalculationStatusUpdater
    {
        private readonly ICalculationRepository _storageRepo = storageRepo;

        public Task UpdateStatus(Calculation calculation, CancellationToken cancellationToken)
        {
            return _storageRepo.UpdateCalculationStatusAsync(new CalculationStatusUpdate(calculation.Id, calculation.UpdatedAt, calculation.Status), cancellationToken);
        }
    }
}
