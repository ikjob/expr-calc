using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.Entities
{
    public readonly record struct CalculationStatusUpdate(Guid Id, DateTime UpdatedAt, CalculationStatus Status);
}
