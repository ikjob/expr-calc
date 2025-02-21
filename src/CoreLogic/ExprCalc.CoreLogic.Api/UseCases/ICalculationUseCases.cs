using ExprCalc.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.CoreLogic.Api.UseCases
{
    /// <summary>
    /// Use cases for Calculations processing
    /// </summary>
    public interface ICalculationUseCases
    {
        Task<List<Calculation>> GetCalculationsListAsync(CancellationToken token);
        Task<Calculation> CreateCalculationAsync(Calculation calculation, CancellationToken token);
        Task<CalculationStatusUpdate> CancelCalculationAsync(Guid id, User cancelledBy, CancellationToken token);
    }
}
