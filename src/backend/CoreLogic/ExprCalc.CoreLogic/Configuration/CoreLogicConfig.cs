﻿using System;
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

        /// <summary>
        /// Registry repopulation after restart happens in batches to avoid selection of large numbers of calculations at once
        /// </summary>
        [Range(1, int.MaxValue)]
        public int RegistryRepopulationBatch { get; init; } = 1000;
        /// <summary>
        /// Delay for repopulation when registry is overflowed
        /// </summary>
        public TimeSpan RegistryRepopulationDelay { get; init; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Priodical cleanup job will remove calculations that are older than specified amount of time
        /// </summary>
        public TimeSpan StorageCleanupExpiration { get; init; } = TimeSpan.FromDays(1);
        /// <summary>
        /// Periods of execution for cleanup job. Zero value means no cleanup
        /// </summary>
        public TimeSpan StorageCleanupPeriod { get; init; } = TimeSpan.FromHours(12);


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
        
            if (RegistryRepopulationDelay <= TimeSpan.Zero)
                yield return new ValidationResult($"RegistryRepopulationDelay cannot be zero or negative", [nameof(RegistryRepopulationDelay)]);
        
            if (StorageCleanupExpiration <= TimeSpan.Zero)
                yield return new ValidationResult($"StorageCleanupExpiration cannot be zero or negative", [nameof(StorageCleanupExpiration)]);
            if (StorageCleanupPeriod < TimeSpan.Zero)
                yield return new ValidationResult($"StorageCleanupPeriod cannot be negative", [nameof(StorageCleanupPeriod)]);
        }
    }
}
