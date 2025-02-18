using ExprCalc.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.CoreLogic.Resources.CalculationsRegistry
{
    internal struct CalculationProcessingGuard : IDisposable
    {
        private ICalculationProcessingFinisher? _finisher;

        public CalculationProcessingGuard(
            Calculation calculation, 
            CancellationToken token, 
            ICalculationProcessingFinisher finisher)
        {
            Calculation = calculation;
            Token = token;
            _finisher = finisher;
        }

        public readonly Calculation Calculation { get; }
        public readonly CancellationToken Token { get; }

        public void Dispose()
        {
            _finisher?.FinishCalculation(Calculation.Id);
            _finisher = null;
        }
    }

    internal interface ICalculationProcessingFinisher
    {
        void FinishCalculation(Guid id);
    }
}
