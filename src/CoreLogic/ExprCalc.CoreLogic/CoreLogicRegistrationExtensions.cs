using ExprCalc.Common.Instrumentation;
using ExprCalc.CoreLogic.Api.UseCases;
using ExprCalc.CoreLogic.Configuration;
using ExprCalc.CoreLogic.Resources.CalculationsRegistry;
using ExprCalc.CoreLogic.Services.CalculationsProcessor;
using ExprCalc.CoreLogic.UseCases;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ExprCalc.CoreLogic
{
    public static class CoreLogicRegistrationExtensions
    {
        public static void AddCoreLogicServices(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddOptions<CoreLogicConfig>()
                .BindConfiguration(CoreLogicConfig.ConfigurationSectionName)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            serviceCollection.AddSingleton<Instrumentation.InstrumentationContainer>();

            serviceCollection.AddSingleton<ICalculationUseCases, CalculationUseCases>();

            serviceCollection.AddSingleton<IScheduledCalculationsRegistry>((provider) => new QueueBasedCalculationsRegistry(ResolveConfig(provider).MaxPendingCalculationsCount));
            serviceCollection.AddHostedService<CalculationsProcessingService>();
        }

        private static CoreLogicConfig ResolveConfig(IServiceProvider provider)
        {
            return provider.GetRequiredService<IOptions<CoreLogicConfig>>().Value;
        }


        public static void AddCoreLogicMetrics(this MetricsRegistry registry)
        {
            registry.Add(Instrumentation.InstrumentationContainer.MeterName);
        }
        public static void AddCoreLogicActivitySources(this ActivitySourcesRegistry registry)
        {
            registry.Add(Instrumentation.InstrumentationContainer.ActivitySourceName);
        }
    }
}
