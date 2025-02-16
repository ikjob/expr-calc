using ExprCalc.CoreLogic.UseCases;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.CoreLogic.Instrumentation
{
    internal class CalculationUseCasesMetrics
    {
        internal CalculationUseCasesMetrics(Meter meter) 
        {
            CreateCalculation = new MethodMetrics(meter, "create_calculation", nameof(CalculationUseCases.CreateCalculationAsync));
            GetCalculationsList = new MethodMetrics(meter, "get_calculations_list", nameof(CalculationUseCases.GetCalculationsListAsync));
        }

        public MethodMetrics GetCalculationsList { get; }
        public MethodMetrics CreateCalculation { get; }
    }
}
