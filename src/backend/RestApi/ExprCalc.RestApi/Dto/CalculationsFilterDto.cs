using ExprCalc.Entities.Enums;
using ExprCalc.Entities.MetadataParams;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.RestApi.Dto
{
    public class CalculationFiltersDto
    {
        [FromQuery]
        public Guid? Id { get; set; }
        [FromQuery]
        public string? CreatedBy { get; init; }

        [FromQuery]
        public DateTime? CreatedAtMin { get; init; }
        [FromQuery]
        public DateTime? CreatedAtMax { get; init; }

        [FromQuery]
        public DateTime? UpdatedAtMin { get; init; }
        [FromQuery]
        public DateTime? UpdatedAtMax { get; init; }

        [FromQuery]
        public CalculationState? State { get; init; }
        [FromQuery]
        public string? Expression { get; init; }


        public CalculationFilters IntoEntity()
        {
            return new CalculationFilters()
            {
                Id = Id,
                CreatedBy = CreatedBy,
                CreatedAtMin = CreatedAtMin,
                CreatedAtMax = CreatedAtMax,
                UpdatedAtMin = UpdatedAtMin,
                UpdatedAtMax = UpdatedAtMax,
                State = State,
                Expression = Expression
            };
        }
    }
}
