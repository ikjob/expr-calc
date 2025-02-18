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
        CalculationRegistryReservedSlot TryReserveSlot();

        Task<Calculation> TakeNext(CancellationToken cancellationToken);
        Task<CalculationProcessingGuard> TakeNextForProcessing(CancellationToken cancellationToken);

        bool Contains(Guid id);
        bool TryGetStatus(Guid id, [NotNullWhen(true)] out CalculationStatus? status);
        bool TryCancel(Guid id, User cancelledBy);
    }
}
