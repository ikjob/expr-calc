using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.Storage.Configuration
{
    public class StorageConfig : IValidatableObject
    {
        public const string ConfigurationSectionName = "Storage";

        /// <summary>
        /// Path to the directory where the database will be created
        /// </summary>
        public string DatabaseDirectory { get; init; } = "./db/";

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            yield break;
        }
    }
}
