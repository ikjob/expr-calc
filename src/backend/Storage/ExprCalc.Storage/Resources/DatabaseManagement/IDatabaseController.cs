using ExprCalc.Entities;
using ExprCalc.Entities.MetadataParams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.Storage.Resources.DatabaseManagement
{
    /// <summary>
    /// Database controller, that provides operations on database
    /// </summary>
    /// <remarks>
    /// This layer of abstraction was implemented specifically to support SQLite database partitioning later
    /// </remarks>
    internal interface IDatabaseController : IDisposable, IAsyncDisposable
    {
        Task Init(CancellationToken token);

        Task<bool> ContainsCalculationAsync(Guid id, CancellationToken token);
        Task<Calculation> GetCalculationByIdAsync(Guid id, CancellationToken token);
        Task<PaginatedResult<Calculation>> GetCalculationsListAsync(CalculationFilters filters, PaginationParams pagination, CancellationToken token);
        Task<Calculation> AddCalculationAsync(Calculation calculation, CancellationToken token);
        Task<bool> TryUpdateCalculationStatusAsync(CalculationStatusUpdate calculationStatus, CancellationToken token);

        Task<int> ResetNonFinalStateToPendingAsync(DateTime maxCreatedAt, DateTime newUpdatedAt, CancellationToken token);
        Task<int> DeleteCalculationsAsync(DateTime createdBefore, CancellationToken token);
        Task<bool> DeleteCalculationByIdAsync(Guid id, CancellationToken token);
    }
}
