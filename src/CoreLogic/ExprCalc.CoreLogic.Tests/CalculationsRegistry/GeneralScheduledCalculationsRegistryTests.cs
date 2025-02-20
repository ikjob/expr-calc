using ExprCalc.CoreLogic.Resources.CalculationsRegistry;
using ExprCalc.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.CoreLogic.Tests.CalculationsRegistry
{
    internal class GeneralScheduledCalculationsRegistryTests
    {
        public static Calculation CreateCalculation(string expression = "1 + 2")
        {
            return Calculation.CreateInitial(expression, new User("test_user"));
        }
        public static Calculation CreateCalculationWithId(Guid id, string expression = "1 + 2")
        {
            return new Calculation(
                id,
                expression,
                new User("test_user"),
                DateTime.UtcNow,
                DateTime.UtcNow,
                CalculationStatus.Pending);
        }

        public static async Task AddTakeTest(IScheduledCalculationsRegistry registry)
        {
            using CancellationTokenSource deadlockProtection = new CancellationTokenSource();
            deadlockProtection.CancelAfter(TimeSpan.FromSeconds(60));

            var calculation = CreateCalculation();

            bool success = registry.TryAdd(calculation, DateTime.UtcNow);
            Assert.True(success);

            var takenCalc = await registry.TakeNext(deadlockProtection.Token);
            Assert.Equal(takenCalc.Id, calculation.Id);
            Assert.Equal(takenCalc, calculation);
        }

        public static async Task AddOverflowTest(IScheduledCalculationsRegistry registry, int maxCount)
        {
            using CancellationTokenSource deadlockProtection = new CancellationTokenSource();
            deadlockProtection.CancelAfter(TimeSpan.FromSeconds(60));

            Calculation calculation;
            bool success;
            for (int i = 0; i < maxCount; i++)
            {
                calculation = CreateCalculation(expression: i.ToString());
                success = registry.TryAdd(calculation, DateTime.UtcNow);
                Assert.True(success);
            }

            calculation = CreateCalculation();
            success = registry.TryAdd(calculation, DateTime.UtcNow);
            Assert.False(success);

            calculation = await registry.TakeNext(deadlockProtection.Token);
            Assert.InRange(int.Parse(calculation.Expression), 0, maxCount);

            calculation = CreateCalculation("0");
            success = registry.TryAdd(calculation, DateTime.UtcNow);
            Assert.True(success);

            for (int i = 0; i < maxCount; i++)
            {
                calculation = await registry.TakeNext(deadlockProtection.Token);
                Assert.InRange(int.Parse(calculation.Expression), 0, maxCount);
            }

            Assert.True(registry.IsEmpty);
        }

        public static async Task KeyUniquenessTest(IScheduledCalculationsRegistry registry)
        {
            using CancellationTokenSource deadlockProtection = new CancellationTokenSource();
            deadlockProtection.CancelAfter(TimeSpan.FromSeconds(60));

            var nonUniqueGuid = Guid.NewGuid();

            Assert.False(registry.Contains(nonUniqueGuid));

            Calculation calculation = CreateCalculationWithId(nonUniqueGuid);
            bool success = registry.TryAdd(calculation, DateTime.UtcNow);
            Assert.True(success);

            Assert.True(registry.Contains(nonUniqueGuid));

            Assert.Throws<DuplicateKeyException>(() =>
            {
                calculation = CreateCalculationWithId(nonUniqueGuid);
                registry.TryAdd(calculation, DateTime.UtcNow);
            });

            Assert.True(registry.Contains(nonUniqueGuid));

            calculation = await registry.TakeNext(deadlockProtection.Token);
            Assert.Equal(nonUniqueGuid, calculation.Id);

            Assert.False(registry.Contains(nonUniqueGuid));
        }

        public static async Task StatusAccessTest(IScheduledCalculationsRegistry registry)
        {
            using CancellationTokenSource deadlockProtection = new CancellationTokenSource();
            deadlockProtection.CancelAfter(TimeSpan.FromSeconds(60));

            Assert.False(registry.TryGetStatus(Guid.NewGuid(), out var status));

            var calculation = CreateCalculation();
            bool success = registry.TryAdd(calculation, DateTime.UtcNow);
            Assert.True(success);

            Assert.True(registry.TryGetStatus(calculation.Id, out status));
            Assert.Equal(calculation.Status.State, status.State);
            Assert.Equal(calculation.Status, status);

            calculation.MakeInProgress();
            calculation.MakeSuccess(100);

            Assert.True(registry.TryGetStatus(calculation.Id, out status));
            Assert.Equal(Entities.Enums.CalculationState.Success, status.State);
            Assert.Equal(calculation.Status, status);

            calculation = await registry.TakeNext(deadlockProtection.Token);
            Assert.Equal(Entities.Enums.CalculationState.Success, calculation.Status.State);

            Assert.True(registry.IsEmpty);
            Assert.False(registry.TryGetStatus(calculation.Id, out status));
        }
    }
}
