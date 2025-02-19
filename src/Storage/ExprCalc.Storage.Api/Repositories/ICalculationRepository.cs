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
        Task<Calculation> CreateCalculationAsync(Calculation calculation, CancellationToken token);
        Task<bool> UpdateCalculationStatusAsync(CalculationStatusUpdate calculationStatusUpdate, CancellationToken token);
    }
}
