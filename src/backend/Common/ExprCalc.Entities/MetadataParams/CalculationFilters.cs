using ExprCalc.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.Entities.MetadataParams
{
    public record class CalculationFilters
    {
        public static CalculationFilters NoFilters { get; } = new CalculationFilters();

        public Guid? Id { get; init; }

        public string? CreatedBy { get; init; }

        public DateTime? CreatedAtMin { get; init; }
        public DateTime? CreatedAtMax { get; init; }

        public DateTime? UpdatedAtMin { get; init; }
        public DateTime? UpdatedAtMax { get; init; }

        public CalculationState? State { get; init; }

        public string? Expression { get; init; }

        public double? CalculationResultMin { get; init; }
        public double? CalculationResultMax { get; init; }
    }
}
