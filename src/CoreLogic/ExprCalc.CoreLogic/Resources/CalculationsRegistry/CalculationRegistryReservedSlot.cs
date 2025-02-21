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

        public CalculationRegistryReservedSlot(ICalculationRegistrySlotFiller slotFiller)
        {
            _slotFiller = slotFiller;
        }

        public readonly bool IsAvailable => _slotFiller != null;

        public void Fill(Calculation calculation, DateTime availableAfter)
        {
            if (_slotFiller == null)
                throw new InvalidOperationException("Unable to fill released slot");

            _slotFiller.FillSlot(calculation, availableAfter);
            _slotFiller = null;
        }

        public void Dispose()
        {
            _slotFiller?.ReleaseSlot();
            _slotFiller = null;
        }
    }


    internal interface ICalculationRegistrySlotFiller
    {
        void FillSlot(Calculation calculation, DateTime availableAfter);
        void ReleaseSlot();
    }
}
