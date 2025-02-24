using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using ExprCalc.ExpressionParsing.Parser;

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
        /// Max number of registered calculations (pending or in progress ones). New ones will be rejected on overflow.
        /// </summary>
        [Range(1, int.MaxValue)]
        public int MaxRegisteredCalculationsCount { get; init; } = 20000;
        /// <summary>
        /// Contains delays for all operations. If some operation is not presented, then it is executed without delay
        /// </summary>
        public Dictionary<ExpressionOperationType, TimeSpan> OperationsTime { get; init; } = new Dictionary<ExpressionOperationType, TimeSpan>();
        /// <summary>
        /// Min delay before the calculation can be taken for processing after it was submitted.
        /// Actual delay sets randomly between <see cref="MinCalculationAvailabilityDelay"/> and <see cref="MaxCalculationAvailabilityDelay"/>.
        /// </summary>
        public TimeSpan MinCalculationAvailabilityDelay { get; init; } = TimeSpan.Zero;
        /// <summary>
        /// Max delay before the calculation can be taken for processing after it was submitted.
        /// Actual delay sets randomly between <see cref="MinCalculationAvailabilityDelay"/> and <see cref="MaxCalculationAvailabilityDelay"/>.
        public TimeSpan MaxCalculationAvailabilityDelay { get; init; } = TimeSpan.FromSeconds(15);

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (CalculationProcessorsCount == 0 || CalculationProcessorsCount < -1)
                yield return new ValidationResult("Number of processors cannot be zero or negative (only '-1' has special meaning)", [nameof(CalculationProcessorsCount)]);

            foreach (var opTime in OperationsTime)
            {
                if (opTime.Value < TimeSpan.Zero)
                    yield return new ValidationResult($"Operation time cannot be negative. Problematic operation: {opTime.Key}", [nameof(OperationsTime)]);
            }

            if (MinCalculationAvailabilityDelay < TimeSpan.Zero)
                yield return new ValidationResult("Min delay cannot be negative", [nameof(MinCalculationAvailabilityDelay)]);
            if (MaxCalculationAvailabilityDelay < TimeSpan.Zero)
                yield return new ValidationResult("Max delay cannot be negative", [nameof(MaxCalculationAvailabilityDelay)]);
            if (MaxCalculationAvailabilityDelay < MinCalculationAvailabilityDelay)
                yield return new ValidationResult("Max delay cannot be less than min delay", [nameof(MinCalculationAvailabilityDelay), nameof(MaxCalculationAvailabilityDelay)]);
        }
    }
}
