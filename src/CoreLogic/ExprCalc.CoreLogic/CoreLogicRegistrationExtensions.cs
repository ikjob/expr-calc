﻿using ExprCalc.Common.Instrumentation;
using ExprCalc.CoreLogic.Api.UseCases;
using ExprCalc.CoreLogic.Configuration;
using ExprCalc.CoreLogic.Services;
using ExprCalc.CoreLogic.UseCases;
using Microsoft.Extensions.DependencyInjection;

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

            serviceCollection.AddHostedService<CalculationsProcessingService>();
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
