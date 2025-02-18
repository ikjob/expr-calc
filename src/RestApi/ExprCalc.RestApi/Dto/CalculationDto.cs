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
        [StringLength(Entities.Calculation.MaxExpressionLength, MinimumLength = 1)]
        public required string Expression { get; set; }
        [Required]
        [StringLength(Entities.User.MaxLoginLength, MinimumLength = 1)]
        [RegularExpression(@"[a-zA-Z0-9_]+")]
        public required string CreatedBy { get; set; }
    }

    public record class CalculationGetDto : CalculationBaseDto
    {
        public required Guid Id { get; set; }
        public required DateTime CreatedAt { get; set; }
        public CalculationStatusDto? Status { get; set; }


        public static CalculationGetDto FromEntity(Entities.Calculation entity)
        {
            return new CalculationGetDto()
            {
                Id = entity.Id,
                Expression = entity.Expression,
                CreatedBy = entity.CreatedBy.Login,
                CreatedAt = entity.CreatedAt,
                Status = entity.Status != null ? CalculationStatusDto.FromEntity(entity.Status, entity.UpdatedAt) : null
            };
        }
    }

    public record class CalculationCreateDto : CalculationBaseDto
    {
        public Entities.Calculation IntoEntity()
        {
            return Entities.Calculation.CreateInitial(Expression, new Entities.User(CreatedBy));
        }
    }
}
