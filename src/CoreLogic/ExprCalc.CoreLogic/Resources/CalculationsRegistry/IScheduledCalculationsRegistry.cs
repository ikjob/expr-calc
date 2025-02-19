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
    /// <summary>
    /// Calculations registry that stores and provides calculation for processors
    /// </summary>
    internal interface IScheduledCalculationsRegistry
    {
        bool TryAdd(Calculation calculation, DateTime availableAfter);
        CalculationRegistryReservedSlot TryReserveSlot(Calculation calculation);

        Task<Calculation> TakeNext(CancellationToken cancellationToken);
        Task<CalculationProcessingGuard> TakeNextForProcessing(CancellationToken cancellationToken);

        bool Contains(Guid id);
        bool TryGetStatus(Guid id, [NotNullWhen(true)] out CalculationStatus? status);
        bool TryCancel(Guid id, User cancelledBy);
    }
}
