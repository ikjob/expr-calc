using ExprCalc.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.CoreLogic.Resources.CalculationsRegistry
{
    /// <summary>
    /// Guard to safely release slot back to the <see cref="IScheduledCalculationsRegistry"/>.
    /// Designed to be used with `using` statement
    /// </summary>
    internal struct CalculationRegistryReservedSlot : IDisposable
    {
        private ICalculationRegistrySlotFiller? _slotFiller;
        private readonly long _reservedMemory;

        public CalculationRegistryReservedSlot(ICalculationRegistrySlotFiller slotFiller, long reservedMemory)
        {
            _slotFiller = slotFiller;
            _reservedMemory = reservedMemory;
        }

        public readonly bool IsAvailable => _slotFiller != null;

        public void Fill(Calculation calculation, TimeSpan delayBeforeExecution)
        {
            if (_slotFiller == null)
                throw new InvalidOperationException("Unable to fill released slot");

            _slotFiller.FillSlot(calculation, delayBeforeExecution);
            _slotFiller = null;
        }

        public void Dispose()
        {
            _slotFiller?.ReleaseSlot(_reservedMemory);
            _slotFiller = null;
        }
    }


    internal interface ICalculationRegistrySlotFiller
    {
        void FillSlot(Calculation calculation, TimeSpan delayBeforeExecution);
        void ReleaseSlot(long reservedMemory);
    }
}
