using ExprCalc.CoreLogic.Resources.CalculationsRegistry;
using ExprCalc.CoreLogic.Resources.ExpressionCalculation;
using ExprCalc.CoreLogic.Services.CalculationsProcessor;
using ExprCalc.Entities;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.CoreLogic.Tests.CalculationsProcessor
{
    public class CalculationsProcessingServiceTests
    {
        private class ExternalStatusUpdaterMock : IExternalCalculationStatusUpdater
        {
            public readonly ConcurrentDictionary<Guid, Calculation> Updates = new ConcurrentDictionary<Guid, Calculation>();

            public Task UpdateStatus(Calculation calculation, CancellationToken cancellationToken)
            {
                Updates[calculation.Id] = calculation;
                return Task.CompletedTask;
            }
        }

        private static (ExpressionCalculator, ExternalStatusUpdaterMock) CreateCalculator(Dictionary<ExpressionParsing.Parser.ExpressionOperationType, TimeSpan>? opTimes = null)
        {
            var updater = new ExternalStatusUpdaterMock();
            var calculator = new ExpressionCalculator(updater,
                Options.Create(new Configuration.CoreLogicConfig()
                {
                    OperationsTime = opTimes ?? []
                }),
                NullLogger<ExpressionCalculator>.Instance);

            return (calculator, updater);
        }

        private static (CalculationsProcessingService, ExternalStatusUpdaterMock, IScheduledCalculationsRegistry) CreateCalculationsProcessingService(int processorCount = 2, int registrySize = 100)
        {
            var registry = new QueueBasedCalculationsRegistry(registrySize, new Instrumentation.ScheduledCalculationsRegistryMetrics(new System.Diagnostics.Metrics.Meter("dummy")));
            var (calculator, updater) = CreateCalculator();

            var calcProcessor = new CalculationsProcessingService(
                calculationsRegistry: registry,
                calculator: calculator,
                config: Options.Create(new Configuration.CoreLogicConfig()
                {
                    CalculationProcessorsCount = processorCount,
                    MaxRegisteredCalculationsCount = registrySize
                }),
                logger: NullLogger<CalculationsProcessingService>.Instance,
                instrumentation: Instrumentation.InstrumentationContainer.CreateNull()
                );

            return (calcProcessor, updater, registry);
        }

        public static Calculation CreateCalculation(string expression = "1 + 2")
        {
            return Calculation.CreateInitial(expression, new User("test_user"));
        }


        // =========


        [Fact]
        public async Task RunExpressionTest()
        {
            using CancellationTokenSource deadlockProtection = new CancellationTokenSource();
            deadlockProtection.CancelAfter(TimeSpan.FromSeconds(60));

            var (processor, updater, registry) = CreateCalculationsProcessingService();

            using (processor)
            {
                await processor.StartAsync(deadlockProtection.Token);

                for (int i = 0; i < 100; i++)
                {
                    var calculation = CreateCalculation("10 / 2");
                    while (!registry.TryAdd(calculation, DateTime.UtcNow))
                        await Task.Delay(10, deadlockProtection.Token);
                }

                while (!registry.IsEmpty)
                    await Task.Delay(10, deadlockProtection.Token);

                Assert.Equal(100, updater.Updates.Count);
                Assert.All(updater.Updates, (item) =>
                {
                    Assert.True(item.Value.Status.IsSuccess(out var successStatus));
                    Assert.Equal(5.0, successStatus.CalculationResult, 1E-8);
                });

                await processor.StopAsync(deadlockProtection.Token);
            }
        }
    }
}
