using ExprCalc.Entities;
using ExprCalc.Storage.Resources.DatabaseManagement;
using ExprCalc.Storage.Resources.SqliteQueries;
using ExprCalc.Storage.Resources.SqliteQueries.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace ExprCalc.Storage.Tests.DatabaseManagement
{
    public class SqliteDbControllerTests
    {
        private static SqliteDbController CreateController(string directory)
        {
            var queryProvider = new SqliteDbQueryProvider();
            return new SqliteDbController(
                queryProvider,
                queryProvider,
                directory,
                logger: NullLogger<SqliteDbController>.Instance);
        }

        private static DateTime RoundTime(DateTime dateTime)
        {
            return DateTime.UnixEpoch + TimeSpan.FromMilliseconds((long)(dateTime - DateTime.UnixEpoch).TotalMilliseconds);
        }
        private static Calculation CreateCalculation(string expression = "1 + 2", CalculationStatus? status = null, TimeSpan? timeOffset = null)
        {
            DateTime createdAt = DateTime.UtcNow;
            if (timeOffset != null)
                createdAt = createdAt.Add(timeOffset.Value);
            createdAt = RoundTime(createdAt);

            return new Calculation(Guid.CreateVersion7(), expression, new User("test_user"), createdAt, createdAt, status ?? CalculationStatus.Pending);
        }
        private static Calculation CreateCalculationSuccess(string expression = "1 + 2", double result = 3, TimeSpan? timeOffset = null)
        {
            return CreateCalculation(expression, CalculationStatus.CreateSuccess(result), timeOffset);
        }
        private static Calculation CreateCalculationFailed(string expression = "1 + 2", TimeSpan? timeOffset = null)
        {
            return CreateCalculation(expression,
                CalculationStatus.CreateFailed(
                    Entities.Enums.CalculationErrorCode.ArithmeticError,
                    new CalculationErrorDetails()
                    { 
                        ErrorCode = CalculationErrorDetails.DivisionByZeroErrorCode,
                        Offset = 10,
                        Length = 1
                    }), timeOffset);

        }
        private static Calculation CreateCalculationCancelled(string expression = "1 + 2", string cancelledBy = "test_user2", TimeSpan? timeOffset = null)
        {
            return CreateCalculation(expression, CalculationStatus.CreateCancelled(new User(cancelledBy)), timeOffset);
        }

        // ===============


        private readonly ITestOutputHelper _output;

        public SqliteDbControllerTests(ITestOutputHelper output)
        {
            _output = output;
        }


        [Fact]
        public async Task InitDisposeTest()
        {
            CancellationTokenSource deadlockProtection = new CancellationTokenSource();
            deadlockProtection.CancelAfter(TimeSpan.FromSeconds(60));

            using var tempDirHolder = new TempDirectoryHolder();
            await using var controller = CreateController(tempDirHolder.Directory);

            await controller.Init(deadlockProtection.Token);
        }

        [Fact]
        public async Task CreateCalculationTest()
        {
            CancellationTokenSource deadlockProtection = new CancellationTokenSource();
            deadlockProtection.CancelAfter(TimeSpan.FromSeconds(60));

            using var tempDirHolder = new TempDirectoryHolder();
            await using var controller = CreateController(tempDirHolder.Directory);
            await controller.Init(deadlockProtection.Token);


            await controller.AddCalculationAsync(CreateCalculation(), deadlockProtection.Token);
            await controller.AddCalculationAsync(CreateCalculationSuccess(), deadlockProtection.Token);
            await controller.AddCalculationAsync(CreateCalculationFailed(), deadlockProtection.Token);
            await controller.AddCalculationAsync(CreateCalculationCancelled(), deadlockProtection.Token);
        }


        [Fact]
        public async Task CreateCalculationDuplicateKeysTest()
        {
            CancellationTokenSource deadlockProtection = new CancellationTokenSource();
            deadlockProtection.CancelAfter(TimeSpan.FromSeconds(60));

            using var tempDirHolder = new TempDirectoryHolder();
            await using var controller = CreateController(tempDirHolder.Directory);
            await controller.Init(deadlockProtection.Token);

            var calculation = CreateCalculation();
            await controller.AddCalculationAsync(calculation, deadlockProtection.Token);

            await Assert.ThrowsAsync<DuplicatePrimaryKeyException>(async () =>
            {
                await controller.AddCalculationAsync(calculation, deadlockProtection.Token);
            });
        }


        [Fact]
        public async Task GetCalculationsListTest()
        {
            CancellationTokenSource deadlockProtection = new CancellationTokenSource();
            deadlockProtection.CancelAfter(TimeSpan.FromSeconds(60));

            using var tempDirHolder = new TempDirectoryHolder();
            await using var controller = CreateController(tempDirHolder.Directory);
            await controller.Init(deadlockProtection.Token);

            List<Calculation> insertionList = [
                CreateCalculation(),
                CreateCalculationSuccess(timeOffset: TimeSpan.FromMilliseconds(10)),
                CreateCalculationFailed(timeOffset: TimeSpan.FromMilliseconds(20)),
                CreateCalculationCancelled(timeOffset: TimeSpan.FromMilliseconds(30))
            ];
            await controller.AddCalculationAsync(insertionList[0], deadlockProtection.Token);
            await controller.AddCalculationAsync(insertionList[1], deadlockProtection.Token);
            await controller.AddCalculationAsync(insertionList[2], deadlockProtection.Token);
            await controller.AddCalculationAsync(insertionList[3], deadlockProtection.Token);


            var list = await controller.GetCalculationsListAsync(deadlockProtection.Token);
            list.Reverse();

            Assert.All(insertionList.Zip(list), (item) =>
            {
                Assert.Equal(item.First.Id, item.Second.Id);
                Assert.Equal(item.First.Status.State, item.Second.Status.State);

                Assert.Equivalent(item.First, item.Second);
            });
        }


        [Fact]
        public async Task GetCalculationByIdTest()
        {
            CancellationTokenSource deadlockProtection = new CancellationTokenSource();
            deadlockProtection.CancelAfter(TimeSpan.FromSeconds(60));

            using var tempDirHolder = new TempDirectoryHolder();
            await using var controller = CreateController(tempDirHolder.Directory);
            await controller.Init(deadlockProtection.Token);

            List<Calculation> insertionList = [
                CreateCalculation(),
                CreateCalculationSuccess(timeOffset: TimeSpan.FromMilliseconds(10)),
                CreateCalculationFailed(timeOffset: TimeSpan.FromMilliseconds(20)),
                CreateCalculationCancelled(timeOffset: TimeSpan.FromMilliseconds(30))
            ];
            await controller.AddCalculationAsync(insertionList[0], deadlockProtection.Token);
            await controller.AddCalculationAsync(insertionList[1], deadlockProtection.Token);
            await controller.AddCalculationAsync(insertionList[2], deadlockProtection.Token);
            await controller.AddCalculationAsync(insertionList[3], deadlockProtection.Token);
            
            for (int i = 0; i < insertionList.Count; i++)
            {
                var calc = await controller.GetCalculationByIdAsync(insertionList[i].Id, deadlockProtection.Token);

                Assert.Equivalent(insertionList[i], calc);
            }
        }

        [Fact]
        public async Task ContainsCalculationTest()
        {
            CancellationTokenSource deadlockProtection = new CancellationTokenSource();
            deadlockProtection.CancelAfter(TimeSpan.FromSeconds(60));

            using var tempDirHolder = new TempDirectoryHolder();
            await using var controller = CreateController(tempDirHolder.Directory);
            await controller.Init(deadlockProtection.Token);

            List<Calculation> insertionList = [
                CreateCalculation(),
                CreateCalculationSuccess(timeOffset: TimeSpan.FromMilliseconds(10)),
                CreateCalculationFailed(timeOffset: TimeSpan.FromMilliseconds(20)),
                CreateCalculationCancelled(timeOffset: TimeSpan.FromMilliseconds(30))
            ];
            await controller.AddCalculationAsync(insertionList[0], deadlockProtection.Token);
            await controller.AddCalculationAsync(insertionList[1], deadlockProtection.Token);
            await controller.AddCalculationAsync(insertionList[2], deadlockProtection.Token);
            await controller.AddCalculationAsync(insertionList[3], deadlockProtection.Token);

            for (int i = 0; i < insertionList.Count; i++)
            {
                var contains = await controller.ContainsCalculationAsync(insertionList[i].Id, deadlockProtection.Token);
                Assert.True(contains);
            }
        }


        [Fact]
        public async Task TryUpdateCalculationStatusTest()
        {
            CancellationTokenSource deadlockProtection = new CancellationTokenSource();
            deadlockProtection.CancelAfter(TimeSpan.FromSeconds(60));

            using var tempDirHolder = new TempDirectoryHolder();
            await using var controller = CreateController(tempDirHolder.Directory);
            await controller.Init(deadlockProtection.Token);

            var calculation = CreateCalculation();
            
            var statusUpdateToInProgress = new CalculationStatusUpdate(calculation.Id, RoundTime(DateTime.UtcNow), CalculationStatus.InProgress);
            Assert.False(await controller.TryUpdateCalculationStatusAsync(statusUpdateToInProgress, deadlockProtection.Token));

            await controller.AddCalculationAsync(calculation, deadlockProtection.Token);
            Assert.True(await controller.TryUpdateCalculationStatusAsync(statusUpdateToInProgress, deadlockProtection.Token));

            Assert.True(calculation.TryChangeStatus(statusUpdateToInProgress.Status, statusUpdateToInProgress.UpdatedAt, out _));
            var calc = await controller.GetCalculationByIdAsync(calculation.Id, deadlockProtection.Token);

            Assert.Equivalent(calculation, calc);

            var statusUpdateToSuccess = new CalculationStatusUpdate(calculation.Id, RoundTime(DateTime.UtcNow), CalculationStatus.CreateSuccess(3));
            Assert.True(await controller.TryUpdateCalculationStatusAsync(statusUpdateToSuccess, deadlockProtection.Token));
            Assert.True(calculation.TryChangeStatus(statusUpdateToSuccess.Status, statusUpdateToSuccess.UpdatedAt, out _));
            calc = await controller.GetCalculationByIdAsync(calculation.Id, deadlockProtection.Token);

            Assert.Equivalent(calculation, calc);
        }


        [Fact]
        public async Task ParallelReadWriteTest()
        {
            CancellationTokenSource deadlockProtection = new CancellationTokenSource();
            deadlockProtection.CancelAfter(TimeSpan.FromSeconds(60));

            using var tempDirHolder = new TempDirectoryHolder();
            await using var controller = CreateController(tempDirHolder.Directory);
            await controller.Init(deadlockProtection.Token);

            Func<Task> backgroundWrite = async () =>
            {
                await Task.Yield();
                for (int i = 0; i < 100; i++)
                {
                    await controller.AddCalculationAsync(CreateCalculation(), deadlockProtection.Token);
                    await Task.Delay(1);
                }
            };

            var bcgTask = backgroundWrite();
            int reads = 0;
            while (!bcgTask.IsCompleted)
            {
                await controller.GetCalculationsListAsync(deadlockProtection.Token);
                reads++;
            }

            await bcgTask;

            _output.WriteLine($"Reads = {reads}");

            var finalList = await controller.GetCalculationsListAsync(deadlockProtection.Token);
            Assert.Equal(100, finalList.Count);
        }
    }
}
