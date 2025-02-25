using ExprCalc.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.Storage.Resources.DatabaseManagement
{
    internal interface IDatabaseController : IDisposable, IAsyncDisposable
    {
        Task Init(CancellationToken token);

        Task<bool> ContainsCalculationAsync(Guid id, CancellationToken token);
        Task<Calculation> GetCalculationByIdAsync(Guid id, CancellationToken token);
        Task<List<Calculation>> GetCalculationsListAsync(CancellationToken token);
        Task<Calculation> AddCalculationAsync(Calculation calculation, CancellationToken token);
        Task<bool> TryUpdateCalculationStatusAsync(CalculationStatusUpdate calculationStatus, CancellationToken token);
    }
}
