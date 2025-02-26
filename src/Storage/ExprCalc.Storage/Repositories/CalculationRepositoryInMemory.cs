using ExprCalc.Entities;
using ExprCalc.Entities.MetadataParams;
using ExprCalc.Storage.Api.Exceptions;
using ExprCalc.Storage.Api.Repositories;
using ExprCalc.Storage.Instrumentation;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.Storage.Repositories
{
    internal class CalculationRepositoryInMemory : ICalculationRepository
    {
        private readonly ILogger<CalculationRepositoryInMemory> _logger;
        private readonly ActivitySource _activitySource;

        private readonly Dictionary<Guid, Calculation> _data;
        private readonly Lock _lock;

        public CalculationRepositoryInMemory(
            ILogger<CalculationRepositoryInMemory> logger,
            InstrumentationContainer instrumentation)
        {
            _logger = logger;
            _activitySource = instrumentation.ActivitySource;
            _data = new Dictionary<Guid, Calculation>();
            _lock = new Lock();
        }

        public Calculation CreateCalculation(Calculation calculation)
        {
            _logger.LogTrace(nameof(CreateCalculation) + " started");
            using var activity = _activitySource.StartActivity(nameof(CalculationRepositoryInMemory) + "." + nameof(CreateCalculation));

            lock (_lock)
            {
                if (_data.ContainsKey(calculation.Id))
                {
                    _logger.LogDebug("Calculation with the same key is already existed. Key = {key}", calculation.Id);
                    activity?.SetStatus(ActivityStatusCode.Error, "Calculation with the same key is already existed");
                    throw new StorageDuplicateEntityException("Calculation with the same key is already existed");
                }

                _data.Add(calculation.Id, calculation.Clone());
                return calculation;
            }
        }

        public void UpdateCalculationStatus(CalculationStatusUpdate calculationStatusUpdate)
        {
            _logger.LogTrace(nameof(UpdateCalculationStatus) + " started");
            using var activity = _activitySource.StartActivity(nameof(CalculationRepositoryInMemory) + "." + nameof(UpdateCalculationStatus));

            lock (_lock)
            {
                if (_data.TryGetValue(calculationStatusUpdate.Id, out var calculation))
                {
                    if (!calculation.TryChangeStatus(calculationStatusUpdate.Status, calculationStatusUpdate.UpdatedAt, out _))
                        throw new InvalidOperationException("Unexpected status change received by InMemoryStorage");
                }
                else
                {
                    throw new StorageEntityNotFoundException($"Can't update calculation status, because it was not found in storage. Id = {calculationStatusUpdate.Id}");
                }
            }
        }

        public List<Calculation> GetCalculationsList()
        {
            _logger.LogTrace(nameof(GetCalculationsList) + " started");
            using var activity = _activitySource.StartActivity(nameof(CalculationRepositoryInMemory) + "." + nameof(GetCalculationsList));

            lock (_lock)
            {
                return _data.Values.Select(o => o.Clone()).ToList();
            }
        }

        public Calculation GetCalculationById(Guid id)
        {
            _logger.LogTrace(nameof(GetCalculationById) + " started");
            using var activity = _activitySource.StartActivity(nameof(CalculationRepositoryInMemory) + "." + nameof(GetCalculationById));

            lock (_lock)
            {
                if (!_data.TryGetValue(id, out var calc))
                    throw new StorageEntityNotFoundException($"Entity for specified key was not found. Id = {id}");
                return calc.Clone();
            }
        }

        public bool ContainsCalculation(Guid id)
        {
            _logger.LogTrace(nameof(ContainsCalculation) + " started");
            using var activity = _activitySource.StartActivity(nameof(CalculationRepositoryInMemory) + "." + nameof(ContainsCalculation));

            lock (_lock)
            {
                return _data.ContainsKey(id);
            }
        }



        public Task<Calculation> AddCalculationAsync(Calculation calculation, CancellationToken token)
        {
            try
            {
                return Task.FromResult(CreateCalculation(calculation));
            }
            catch (Exception ex)
            {
                return Task.FromException<Calculation>(ex);
            }
        }

        public Task<Calculation> GetCalculationByIdAsync(Guid id, CancellationToken token)
        {
            try
            {
                return Task.FromResult(GetCalculationById(id));
            }
            catch (Exception ex)
            {
                return Task.FromException<Calculation>(ex);
            }
        }

        public Task<bool> ContainsCalculationAsync(Guid id, CancellationToken token)
        {
            try
            {
                return Task.FromResult(ContainsCalculation(id));
            }
            catch (Exception ex)
            {
                return Task.FromException<bool>(ex);
            }
        }

        public Task<PaginatedResult<Calculation>> GetCalculationsListAsync(CalculationFilters filters, PaginationParams pagination, CancellationToken token)
        {
            try
            {
                return Task.FromResult(new PaginatedResult<Calculation>(GetCalculationsList(), 0, uint.MaxValue));
            }
            catch (Exception ex)
            {
                return Task.FromException<PaginatedResult<Calculation>>(ex);
            }
        }

        public Task UpdateCalculationStatusAsync(CalculationStatusUpdate calculationStatusUpdate, CancellationToken token)
        {
            try
            {
                UpdateCalculationStatus(calculationStatusUpdate);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                return Task.FromException<bool>(ex);
            }
        }

        public Task<int> ResetNonFinalStateToPendingAsync(DateTime maxCreatedAt, DateTime newUpdatedAt, CancellationToken token)
        {
            return Task.FromResult(0);
        }

        public Task<int> DeleteCalculationsAsync(DateTime createdBefore, CancellationToken token)
        {
            return Task.FromResult(0);
        }
    }
}
