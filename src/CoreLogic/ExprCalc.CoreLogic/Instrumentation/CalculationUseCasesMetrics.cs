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
            GetCalculationsList = new MethodMetrics(meter, "get_calculations_list", nameof(CalculationUseCases.GetCalculationsListAsync));
            GetCalculationById = new MethodMetrics(meter, "get_calculation_by_id", nameof(CalculationUseCases.GetCalculationByIdAsync));
            CreateCalculation = new MethodMetrics(meter, "create_calculation", nameof(CalculationUseCases.CreateCalculationAsync));
            CancelCalculation = new MethodMetrics(meter, "cancel_calculation", nameof(CalculationUseCases.CancelCalculationAsync));
        }

        public MethodMetrics GetCalculationsList { get; }
        public MethodMetrics GetCalculationById { get; }
        public MethodMetrics CreateCalculation { get; }
        public MethodMetrics CancelCalculation { get; }
    }
}
