using ExprCalc.Entities;
using ExprCalc.Entities.MetadataParams;
using ExprCalc.Storage.Api.Exceptions;
using ExprCalc.Storage.Api.Repositories;
using ExprCalc.Storage.Instrumentation;
using ExprCalc.Storage.Resources.DatabaseManagement;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.Storage.Repositories
{
    internal class CalculationRepositoryInDb : ICalculationRepository
    {
        private readonly IDatabaseController _databaseController;

        private readonly ILogger<CalculationRepositoryInDb> _logger;
        private readonly ActivitySource _activitySource;


        public CalculationRepositoryInDb(
            IDatabaseController databaseController,
            ILogger<CalculationRepositoryInDb> logger,
            InstrumentationContainer instrumentation)
        {
            _databaseController = databaseController;

            _logger = logger;
            _activitySource = instrumentation.ActivitySource;
        }


        public async Task<bool> ContainsCalculationAsync(Guid id, CancellationToken token)
        {
            _logger.LogTrace(nameof(ContainsCalculationAsync) + " started");
            using var activity = _activitySource.StartActivity(nameof(CalculationRepositoryInDb) + "." + nameof(ContainsCalculationAsync));

            try
            {
                return await _databaseController.ContainsCalculationAsync(id, token);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "{methodName} ended with exception", nameof(ContainsCalculationAsync));
                activity?.SetStatus(ActivityStatusCode.Error, $"{nameof(ContainsCalculationAsync)} ended with exception");
                throw;
            }
        }

        public async Task<Calculation> GetCalculationByIdAsync(Guid id, CancellationToken token)
        {
            _logger.LogTrace(nameof(GetCalculationByIdAsync) + " started");
            using var activity = _activitySource.StartActivity(nameof(CalculationRepositoryInDb) + "." + nameof(GetCalculationByIdAsync));

            try
            {
                return await _databaseController.GetCalculationByIdAsync(id, token);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "{methodName} ended with exception", nameof(GetCalculationByIdAsync));
                activity?.SetStatus(ActivityStatusCode.Error, $"{nameof(GetCalculationByIdAsync)} ended with exception");
                throw;
            }
        }

        public async Task<PaginatedResult<Calculation>> GetCalculationsListAsync(CalculationFilters filters, PaginationParams pagination, CancellationToken token)
        {
            _logger.LogTrace(nameof(GetCalculationsListAsync) + " started");
            using var activity = _activitySource.StartActivity(nameof(CalculationRepositoryInDb) + "." + nameof(GetCalculationsListAsync));

            try
            {
                return await _databaseController.GetCalculationsListAsync(filters, pagination, token);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "{methodName} ended with exception", nameof(GetCalculationsListAsync));
                activity?.SetStatus(ActivityStatusCode.Error, $"{nameof(GetCalculationsListAsync)} ended with exception");
                throw;
            }
        }
        public async Task<Calculation> AddCalculationAsync(Calculation calculation, CancellationToken token)
        {
            _logger.LogTrace(nameof(AddCalculationAsync) + " started");
            using var activity = _activitySource.StartActivity(nameof(CalculationRepositoryInDb) + "." + nameof(AddCalculationAsync));

            try
            {
                return await _databaseController.AddCalculationAsync(calculation, token);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "{methodName} ended with exception", nameof(AddCalculationAsync));
                activity?.SetStatus(ActivityStatusCode.Error, $"{nameof(AddCalculationAsync)} ended with exception");
                throw;
            }
        }

        public async Task UpdateCalculationStatusAsync(CalculationStatusUpdate calculationStatusUpdate, CancellationToken token)
        {
            _logger.LogTrace(nameof(UpdateCalculationStatusAsync) + " started");
            using var activity = _activitySource.StartActivity(nameof(CalculationRepositoryInDb) + "." + nameof(UpdateCalculationStatusAsync));

            try
            {
                if (!await _databaseController.TryUpdateCalculationStatusAsync(calculationStatusUpdate, token))
                    throw new StorageEntityNotFoundException($"Calculation status cannot be updated because calculation with sepcified id was not found. Id = {calculationStatusUpdate.Id}");
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "{methodName} ended with exception", nameof(UpdateCalculationStatusAsync));
                activity?.SetStatus(ActivityStatusCode.Error, $"{nameof(UpdateCalculationStatusAsync)} ended with exception");
                throw;
            }
        }

        public async Task<int> ResetNonFinalStateToPendingAsync(DateTime maxCreatedAt, DateTime newUpdatedAt, CancellationToken token)
        {
            _logger.LogTrace(nameof(ResetNonFinalStateToPendingAsync) + " started");
            using var activity = _activitySource.StartActivity(nameof(CalculationRepositoryInDb) + "." + nameof(ResetNonFinalStateToPendingAsync));

            try
            {
                return await _databaseController.ResetNonFinalStateToPendingAsync(maxCreatedAt, newUpdatedAt, token);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "{methodName} ended with exception", nameof(ResetNonFinalStateToPendingAsync));
                activity?.SetStatus(ActivityStatusCode.Error, $"{nameof(ResetNonFinalStateToPendingAsync)} ended with exception");
                throw;
            }
        }

        public async Task<int> DeleteCalculationsAsync(DateTime createdBefore, CancellationToken token)
        {
            _logger.LogTrace(nameof(DeleteCalculationsAsync) + " started");
            using var activity = _activitySource.StartActivity(nameof(CalculationRepositoryInDb) + "." + nameof(DeleteCalculationsAsync));

            try
            {
                return await _databaseController.DeleteCalculationsAsync(createdBefore, token);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "{methodName} ended with exception", nameof(DeleteCalculationsAsync));
                activity?.SetStatus(ActivityStatusCode.Error, $"{nameof(DeleteCalculationsAsync)} ended with exception");
                throw;
            }
        }

        public async Task<bool> DeleteCalculationByIdAsync(Guid id, CancellationToken token)
        {
            _logger.LogTrace(nameof(DeleteCalculationByIdAsync) + " started");
            using var activity = _activitySource.StartActivity(nameof(CalculationRepositoryInDb) + "." + nameof(DeleteCalculationByIdAsync));

            try
            {
                return await _databaseController.DeleteCalculationByIdAsync(id, token);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "{methodName} ended with exception", nameof(DeleteCalculationByIdAsync));
                activity?.SetStatus(ActivityStatusCode.Error, $"{nameof(DeleteCalculationByIdAsync)} ended with exception");
                throw;
            }
        }
    }
}
