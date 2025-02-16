using ExprCalc.Common.Instrumentation;
using ExprCalc.Storage.Api.Repositories;
using ExprCalc.Storage.Configuration;
using ExprCalc.Storage.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace ExprCalc.Storage
{
    public static class StorageRegistrationExtensions
    {
        public static void AddStorageServices(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddOptions<StorageConfig>().BindConfiguration(StorageConfig.ConfigurationSectionName);

            serviceCollection.AddSingleton<Instrumentation.InstrumentationContainer>();

            serviceCollection.AddSingleton<ICalculationRepository, CalculationRepositoryInMemory>();
        }


        public static void AddStorageMetrics(this MetricsRegistry registry)
        {
            registry.Add(Instrumentation.InstrumentationContainer.MeterName);
        }
        public static void AddStorageActivitySources(this ActivitySourcesRegistry registry)
        {
            registry.Add(Instrumentation.InstrumentationContainer.ActivitySourceName);
        }
    }
}
