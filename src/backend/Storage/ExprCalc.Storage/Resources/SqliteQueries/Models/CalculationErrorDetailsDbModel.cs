using ExprCalc.Entities;
using ExprCalc.Storage.Resources.SqliteQueries.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

namespace ExprCalc.Storage.Resources.SqliteQueries.Models
{
    internal readonly struct CalculationErrorDetailsDbModel(string jsonDeatils)
    {
        private static readonly JsonTypeInfo<CalculationErrorDetails> _jsonTypeInfo = GetJsonTypeInfo();
        private static JsonTypeInfo<CalculationErrorDetails> GetJsonTypeInfo()
        {
            if (!JsonSerializerOptions.Default.TryGetTypeInfo(typeof(CalculationErrorDetails), out var tpInfo))
                throw new InvalidOperationException($"{nameof(CalculationErrorDetails)} should be serializable to JSON");

            return (JsonTypeInfo<CalculationErrorDetails>)tpInfo;
        }

        public string JsonDetails { get; init; } = jsonDeatils;

        public static CalculationErrorDetailsDbModel FromEntity(CalculationErrorDetails entity)
        {
            return new CalculationErrorDetailsDbModel()
            {
                JsonDetails = JsonSerializer.Serialize(entity, _jsonTypeInfo)
            };
        }
        public CalculationErrorDetails IntoEntity()
        {
            return JsonSerializer.Deserialize<CalculationErrorDetails>(JsonDetails, _jsonTypeInfo) 
                ?? throw new EntityCorruptedException("Error details should be deserializable from JSON");
        }
    }
}
