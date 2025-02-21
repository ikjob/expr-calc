using ExprCalc.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.RestApi.Dto
{
    public class CalculationStatusUpdateDto
    {
        public required Guid Id { get; set; }
        public required DateTime UpdatedAt { get; set; }
        public required CalculationStatusDto Status { get; set; }

        public static CalculationStatusUpdateDto FromEntity(CalculationStatusUpdate entity)
        {
            return new CalculationStatusUpdateDto()
            {
                Id = entity.Id,
                UpdatedAt = entity.UpdatedAt,
                Status = CalculationStatusDto.FromEntity(entity.Status)
            };
        }
    }
}
