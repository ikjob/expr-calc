using ExprCalc.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.Storage.Api.Repositories
{
    /// <summary>
    /// Repository to interact with calculations data inside storage
    /// </summary>
    public interface ICalculationRepository
    {
        Task<List<Calculation>> GetCalculationsListAsync(CancellationToken token);
        Task<Calculation> GetCalculationByIdAsync(Guid id, CancellationToken token);
        Task<bool> ContainsCalculationAsync(Guid id, CancellationToken token);
        Task<Calculation> AddCalculationAsync(Calculation calculation, CancellationToken token);
        Task UpdateCalculationStatusAsync(CalculationStatusUpdate calculationStatusUpdate, CancellationToken token);
    }
}
