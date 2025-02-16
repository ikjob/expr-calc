using ExprCalc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.RestApi.Dto
{
    public abstract record class CalculationBaseDto
    {
        [Required]
        public required string Expression { get; set; } 
    }

    public record class CalculationGetDto : CalculationBaseDto
    {
        public required Guid Id { get; set; }
        public required DateTime CreatedAt { get; set; }


        public static CalculationGetDto FromEntity(Entities.Calculation entity)
        {
            return new CalculationGetDto()
            {
                Id = entity.Id,
                Expression = entity.Expression,
                CreatedAt = entity.CreatedAt
            };
        }
    }

    public record class CalculationCreateDto : CalculationBaseDto
    {
        public Entities.Calculation IntoEntity()
        {
            return new Entities.Calculation()
            {
                Id = Guid.Empty,
                Expression = Expression,
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}
