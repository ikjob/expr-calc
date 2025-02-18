using ExprCalc.Entities;
using ExprCalc.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.CoreLogic.Resources.CalculationsRegistry
{
    internal interface IScheduledCalculationsRegistry : IDisposable
    {
        bool TryAdd(Calculation calculation, DateTime availableAfter);
        Task<Calculation> TakeNext(CancellationToken cancellationToken);
        Task<CalculationProcessingGuard> TakeNextForProcessing(CancellationToken cancellationToken);

        bool Contains(Guid id);
        bool TryGetState(Guid id, [NotNullWhen(true)] out CalculationState? state);
        bool TryCancel(Guid id, User cancelledBy);
    }
}
