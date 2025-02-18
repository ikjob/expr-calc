using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ExprCalc.CoreLogic.Configuration
{
    public class CoreLogicConfig : IValidatableObject
    {
        public const string ConfigurationSectionName = "CoreLogic";

        /// <summary>
        /// Number of background processor
        /// </summary>
        /// <remarks>
        /// '-1' has special meaning: it starts the number of processors, equals to the number of cores
        /// </remarks>
        public int CalculationProcessorsCount { get; init; } = -1;
        /// <summary>
        /// Max number of pending calculations. New ones will be discarded on overflow.
        /// </summary>
        [Range(1, int.MaxValue)]
        public int MaxPendingCalculationsCount { get; init; } = 20000;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (CalculationProcessorsCount == 0 || CalculationProcessorsCount < -1)
                yield return new ValidationResult("Number of processors cannot be zero or negative (only '-1' has special meaning)", [nameof(CalculationProcessorsCount)]);
        }
    }
}
