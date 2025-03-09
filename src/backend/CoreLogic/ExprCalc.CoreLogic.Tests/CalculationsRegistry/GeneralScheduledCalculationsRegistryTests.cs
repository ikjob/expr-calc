using ExprCalc.CoreLogic.Resources.CalculationsRegistry;
using ExprCalc.Entities;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading;
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

            bool success = registry.TryAdd(calculation, TimeSpan.Zero);
            Assert.True(success);
            Assert.False(registry.IsEmpty);

            var takenCalc = await registry.TakeNext(deadlockProtection.Token);
            Assert.Equal(takenCalc.Id, calculation.Id);
            Assert.Equal(takenCalc, calculation);
            Assert.True(registry.IsEmpty);
        }

        public static void TryTakeTest(IScheduledCalculationsRegistry registry)
        {
            var calculation = CreateCalculation();

            Assert.False(registry.TryTakeNext(out _));

            bool success = registry.TryAdd(calculation, TimeSpan.Zero);
            Assert.True(success);
            Assert.False(registry.IsEmpty);

            Assert.True(registry.TryTakeNext(out var takenCalc));
            Assert.Equal(takenCalc.Id, calculation.Id);
            Assert.Equal(takenCalc, calculation);
            Assert.True(registry.IsEmpty);

            Assert.False(registry.TryTakeNext(out _));
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
                success = registry.TryAdd(calculation, TimeSpan.Zero);
                Assert.True(success);
            }

            calculation = CreateCalculation();
            success = registry.TryAdd(calculation, TimeSpan.Zero);
            Assert.False(success);

            calculation = await registry.TakeNext(deadlockProtection.Token);
            Assert.InRange(int.Parse(calculation.Expression), 0, maxCount);

            calculation = CreateCalculation("0");
            success = registry.TryAdd(calculation, TimeSpan.Zero);
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
            bool success = registry.TryAdd(calculation, TimeSpan.Zero);
            Assert.True(success);

            Assert.True(registry.Contains(nonUniqueGuid));

            Assert.Throws<DuplicateKeyException>(() =>
            {
                calculation = CreateCalculationWithId(nonUniqueGuid);
                registry.TryAdd(calculation, TimeSpan.Zero);
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
            bool success = registry.TryAdd(calculation, TimeSpan.Zero);
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


        public static async Task SlotReservationTest(IScheduledCalculationsRegistry registry)
        {
            using CancellationTokenSource deadlockProtection = new CancellationTokenSource();
            deadlockProtection.CancelAfter(TimeSpan.FromSeconds(60));

            var calculation = CreateCalculation();
            using (var slot = registry.TryReserveSlot(calculation))
            {
                Assert.True(slot.IsAvailable);
                Assert.True(registry.IsEmpty);

                Assert.False(registry.TryTakeNext(out _));

                slot.Fill(calculation, TimeSpan.Zero);
            }

            Assert.False(registry.IsEmpty);

            var takenCalculation = await registry.TakeNext(deadlockProtection.Token);
            Assert.Equal(calculation, takenCalculation);
            Assert.True(registry.IsEmpty);
        }

        public static async Task SlotReservationExtendedTest(IScheduledCalculationsRegistry registry, int maxCount)
        {
            using CancellationTokenSource deadlockProtection = new CancellationTokenSource();
            deadlockProtection.CancelAfter(TimeSpan.FromSeconds(60));

            Calculation calculation;
            for (int i = 0; i < maxCount - 1; i++)
            {
                calculation = CreateCalculation(i.ToString());
                using (var slot = registry.TryReserveSlot(calculation))
                {
                    Assert.True(slot.IsAvailable);
                    slot.Fill(calculation, TimeSpan.Zero);
                }
            }

            calculation = CreateCalculation("-1");
            using (var slot = registry.TryReserveSlot(calculation))
            {
                Assert.True(slot.IsAvailable);
            }


            calculation = CreateCalculation((maxCount - 1).ToString());
            using (var slot = registry.TryReserveSlot(calculation))
            {
                Assert.True(slot.IsAvailable);

                var calculation2 = CreateCalculation("-1");
                using (var slot2 = registry.TryReserveSlot(calculation2))
                {
                    Assert.False(slot2.IsAvailable);
                    Assert.ThrowsAny<InvalidOperationException>(() =>
                    {
                        slot2.Fill(calculation2, TimeSpan.Zero);
                    });
                }
                slot.Fill(calculation, TimeSpan.Zero);
            }


            calculation = CreateCalculation("-1");
            using (var slot = registry.TryReserveSlot(calculation))
            {
                Assert.False(slot.IsAvailable);
            }

            for (int i = 0; i < maxCount; i++)
            {
                var takenCalculation = await registry.TakeNext(deadlockProtection.Token);
                Assert.InRange(int.Parse(takenCalculation.Expression), 0, maxCount);
            }
            Assert.True(registry.IsEmpty);
        }

        public static async Task KeepForProcessingTimeTest(IScheduledCalculationsRegistry registry, int maxCount)
        {
            using CancellationTokenSource deadlockProtection = new CancellationTokenSource();
            deadlockProtection.CancelAfter(TimeSpan.FromSeconds(60));

            Calculation calculation;
            bool success;
            for (int i = 0; i < maxCount; i++)
            {
                calculation = CreateCalculation(i.ToString());
                success = registry.TryAdd(calculation, TimeSpan.Zero);
                Assert.True(success);
            }


            using (var itemForProcessing = await registry.TakeNextForProcessing(deadlockProtection.Token))
            {
                Assert.False(itemForProcessing.Token.IsCancellationRequested);
                Assert.InRange(int.Parse(itemForProcessing.Calculation.Expression), 0, maxCount);

                var tmpCalculation = CreateCalculation("-1");
                bool tmpSuccess = registry.TryAdd(tmpCalculation, TimeSpan.Zero);
                Assert.False(tmpSuccess);

                using (var itemForProcessing2 = await registry.TakeNextForProcessing(deadlockProtection.Token))
                {
                    Assert.False(itemForProcessing2.Token.IsCancellationRequested);
                    Assert.InRange(int.Parse(itemForProcessing2.Calculation.Expression), 0, maxCount);

                    tmpCalculation = CreateCalculation("-1");
                    using (var slot = registry.TryReserveSlot(tmpCalculation))
                    {
                        Assert.False(slot.IsAvailable);
                    }
                }

                tmpCalculation = CreateCalculation("0");
                tmpSuccess = registry.TryAdd(tmpCalculation, TimeSpan.Zero);
                Assert.True(tmpSuccess);
            }

            calculation = CreateCalculation("0");
            success = registry.TryAdd(calculation, TimeSpan.Zero);
            Assert.True(success);

            for (int i = 0; i < maxCount; i++)
            {
                var takenCalculation = await registry.TakeNext(deadlockProtection.Token);
                Assert.InRange(int.Parse(takenCalculation.Expression), 0, maxCount);
            }
            Assert.True(registry.IsEmpty);
        }


        public static async Task TakeNextWaitingTest(IScheduledCalculationsRegistry registry)
        {
            using CancellationTokenSource deadlockProtection = new CancellationTokenSource();
            deadlockProtection.CancelAfter(TimeSpan.FromSeconds(60));

            Assert.True(registry.IsEmpty);

            var calculation = CreateCalculation();
            var bckgTask = Task.Delay(400).ContinueWith(t => registry.TryAdd(calculation, TimeSpan.Zero));

            var takenCalculation = await registry.TakeNext(deadlockProtection.Token);
            Assert.Equal(calculation, takenCalculation);

            Assert.True(await bckgTask);
        }

        public static async Task TryCancelTest(IScheduledCalculationsRegistry registry)
        {
            using CancellationTokenSource deadlockProtection = new CancellationTokenSource();
            deadlockProtection.CancelAfter(TimeSpan.FromSeconds(60));

            var calculation = CreateCalculation();
            bool success = registry.TryAdd(calculation, TimeSpan.Zero);
            Assert.True(success);


            using (var proc = await registry.TakeNextForProcessing(deadlockProtection.Token))
            {
                Assert.False(proc.Token.IsCancellationRequested);
                Assert.Equal(calculation, proc.Calculation);

                Assert.True(registry.Contains(calculation.Id));
                Assert.True(registry.TryGetStatus(calculation.Id, out var status));
                Assert.Equal(Entities.Enums.CalculationState.Pending, status.State);

                proc.Calculation.MakeInProgress();

                Assert.True(registry.Contains(calculation.Id));
                Assert.True(registry.TryGetStatus(calculation.Id, out status));
                Assert.Equal(Entities.Enums.CalculationState.InProgress, status.State);


                Assert.True(registry.TryCancel(calculation.Id, new User("abc"), out var statusUpdate));
                Assert.Equal(calculation.Status, statusUpdate.Value.Status);
                Assert.Equal(Entities.Enums.CalculationState.Cancelled, statusUpdate.Value.Status.State);
                Assert.Equal(calculation.Id, statusUpdate.Value.Id);

                Assert.True(registry.Contains(calculation.Id));
                Assert.True(registry.TryGetStatus(calculation.Id, out status));
                Assert.Equal(Entities.Enums.CalculationState.Cancelled, status.State);

                Assert.True(calculation.Status.IsCancelled(out var cancelledStatus));
                Assert.Equal("abc", cancelledStatus.CancelledBy.Login);

                Assert.True(proc.Token.IsCancellationRequested);
            }

            Assert.False(registry.Contains(calculation.Id));
            Assert.False(registry.TryGetStatus(calculation.Id, out var status2));
            Assert.False(registry.TryCancel(calculation.Id, new User("bcd"), out _));
        }

        public static void EnumerateTest(IScheduledCalculationsRegistry registry)
        {
            var calculation = CreateCalculation();
            bool success = registry.TryAdd(calculation, TimeSpan.Zero);
            Assert.True(success);

            Assert.Single(registry.Enumerate(withCancelled: false));
            Assert.Equal(calculation.Id, registry.Enumerate(withCancelled: false).Single().Id);

            var calculation2 = CreateCalculation();
            registry.TryAdd(calculation2, TimeSpan.Zero);

            Assert.Equal(2, registry.Enumerate(withCancelled: false).Count());

            registry.TryCancel(calculation.Id, new User("abc"), out _);

            Assert.Single(registry.Enumerate(withCancelled: false));
            Assert.Equal(calculation2.Id, registry.Enumerate(withCancelled: false).Single().Id);

            Assert.Equal(2, registry.Enumerate(withCancelled: true).Count());
        }

        public static async Task MultithreadTest(IScheduledCalculationsRegistry registry, int addThreads, int takeThreads, int testItemCount, int addDelay, int takeDelay, int maxItemDelayMs)
        {
            using CancellationTokenSource takeEndTokenSource = new CancellationTokenSource();
            takeEndTokenSource.CancelAfter(TimeSpan.FromSeconds(120));

            Random globalRandom = new Random();
            List<Guid> combinedAddedGuid = new List<Guid>(testItemCount);
            List<Guid> combinedTakenGuid = new List<Guid>(testItemCount);
            Lock @lock = new Lock();

            Func<Task> addAction = async () =>
            {
                await Task.Yield();

                Assert.True(TaskScheduler.Current == TaskScheduler.Default);

                int maxCountForThread = testItemCount / addThreads;
                List<Guid> addedGuids = new List<Guid>(maxCountForThread);

                int seed = 0;
                lock (@lock) { seed = globalRandom.Next(); }
                Random localRandom = new Random(seed);

                while (addedGuids.Count < maxCountForThread)
                {
                    bool avaiable = false;
                    var calculation = CreateCalculation();
                    using (var slot = registry.TryReserveSlot(calculation))
                    {
                        if (slot.IsAvailable)
                        {
                            slot.Fill(calculation, TimeSpan.FromMilliseconds(localRandom.Next(maxItemDelayMs)));
                            addedGuids.Add(calculation.Id);
                            avaiable = true;
                        }
                    }
                    int delay = localRandom.Next(0, addDelay);
                    if (delay > 1)
                        await Task.Delay(delay);
                    else if (!avaiable)
                        await Task.Yield();
                }

                lock (@lock)
                {
                    combinedAddedGuid.AddRange(addedGuids);
                }
            };


            Func<int, Task> takeAction = async (int threadId) =>
            {
                await Task.Yield();

                Assert.True(TaskScheduler.Current == TaskScheduler.Default);

                List<Guid> takenGuids = new List<Guid>(testItemCount);

                int seed = 0;
                lock (@lock) { seed = globalRandom.Next(); }
                Random localRandom = new Random(seed);

                try
                {
                    while (!takeEndTokenSource.IsCancellationRequested)
                    {
                        using (var procItem = await registry.TakeNextForProcessing(takeEndTokenSource.Token))
                        {
                            takenGuids.Add(procItem.Calculation.Id);
                        }
                        int delay = localRandom.Next(0, takeDelay);
                        if (delay > 5)
                            await Task.Delay(delay);
                    }
                }
                catch (OperationCanceledException)
                {
                }

                while (registry.TryTakeNext(out var finalCalc))
                {
                    takenGuids.Add(finalCalc.Id);
                }

                lock (@lock)
                {
                    combinedTakenGuid.AddRange(takenGuids);
                }
            };


            List<Task> takeTasks = new List<Task>(takeThreads);
            List<Task> addTasks = new List<Task>(addThreads);

            for (int i = 0; i < takeThreads; i++)
                takeTasks.Add(takeAction(i));

            for (int i = 0; i < addThreads; i++)
                addTasks.Add(addAction());

            await Task.WhenAll(addTasks);
            takeEndTokenSource.Cancel();
            await Task.WhenAll(takeTasks);

            // Take remaining
            if (!registry.IsEmpty)
            {
                using var remainingCancellation = new CancellationTokenSource();
                remainingCancellation.CancelAfter(maxItemDelayMs * 8);

                while (!registry.IsEmpty)
                {
                    combinedTakenGuid.Add((await registry.TakeNext(remainingCancellation.Token)).Id);
                }
            }

            combinedAddedGuid.Sort();
            combinedTakenGuid.Sort();
            Assert.Equal(combinedAddedGuid.Count, combinedTakenGuid.Count);
            Assert.Equal(combinedAddedGuid, combinedTakenGuid);

            Assert.True(registry.IsEmpty);
        }
    }
}
