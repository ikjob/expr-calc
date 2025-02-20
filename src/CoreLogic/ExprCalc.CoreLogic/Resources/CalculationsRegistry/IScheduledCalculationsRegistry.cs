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
        /// <summary>
        /// Attempts to add new calculation to the registry. Can be rejected if registry is overflowed.
        /// </summary>
        bool TryAdd(Calculation calculation, DateTime availableAfter);
        /// <summary>
        /// Reserve free space inside registry, that can be filled later. 
        /// Main idea is to allow 2-phase commit strategy between registry and storage.
        /// </summary>
        CalculationRegistryReservedSlot TryReserveSlot(Calculation calculation);

        /// <summary>
        /// Removes and returns next available calculation from the registry
        /// </summary>
        Task<Calculation> TakeNext(CancellationToken cancellationToken);
        /// <summary>
        /// Returns next available calculation from the registry, but not removes it,
        /// so it can be cancelled through the registry. 
        /// Calculation will be removed when <see cref="CalculationProcessingGuard.Dispose"/> will be called.
        /// </summary>
        Task<CalculationProcessingGuard> TakeNextForProcessing(CancellationToken cancellationToken);

        /// <summary>
        /// Checks whether the <see cref="Calculation"/> with specified key presented inside registry
        /// </summary>
        bool Contains(Guid id);
        /// <summary>
        /// Attempts to get the <see cref="Calculation"/> status from the registry.
        /// Success if <see cref="Calculation"/> for the specified key is presented.
        /// </summary>
        bool TryGetStatus(Guid id, [NotNullWhen(true)] out CalculationStatus? status);
        /// <summary>
        /// Attemtps to cancel <see cref="Calculation"/>.
        /// Success if <see cref="Calculation"/> for the specified key is presented and its status is not final
        /// </summary>
        bool TryCancel(Guid id, User cancelledBy, [NotNullWhen(true)] out CalculationStatusUpdate? status);
    }
}
