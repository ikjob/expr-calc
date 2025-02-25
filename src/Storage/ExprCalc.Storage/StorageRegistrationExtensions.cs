using ExprCalc.Common.Instrumentation;
using ExprCalc.Storage.Api.Repositories;
using ExprCalc.Storage.Configuration;
using ExprCalc.Storage.Repositories;
using ExprCalc.Storage.Resources.DatabaseManagement;
using ExprCalc.Storage.Resources.SqliteQueries;
using ExprCalc.Storage.Services.DatabaseMonitoring;
using Microsoft.Extensions.DependencyInjection;

namespace ExprCalc.Storage
{
    public static class StorageRegistrationExtensions
    {
        public static void AddStorageServices(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddOptions<StorageConfig>()
                .BindConfiguration(StorageConfig.ConfigurationSectionName)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            serviceCollection.AddSingleton<Instrumentation.InstrumentationContainer>();

            serviceCollection.AddSingleton<ICalculationRepository, CalculationRepositoryInMemory>();

            serviceCollection.AddSingleton<SqliteDbQueryProvider>();
            serviceCollection.AddSingleton<ISqlDbInitializationQueryProvider>(sp => sp.GetRequiredService<SqliteDbQueryProvider>());
            serviceCollection.AddSingleton<ISqlDbCalculationsQueryProvider>(sp => sp.GetRequiredService<SqliteDbQueryProvider>());
            serviceCollection.AddSingleton<IDatabaseController, SqliteDbController>();

            serviceCollection.AddHostedService<DatabaseMonitoringService>();
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
