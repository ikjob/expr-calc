using ExprCalc.Entities;
using ExprCalc.Entities.MetadataParams;
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
        private static Calculation CreateCalculation(string expression = "1 + 2", CalculationStatus? status = null, string? user = null, TimeSpan? timeOffset = null)
        {
            DateTime createdAt = DateTime.UtcNow;
            if (timeOffset != null)
                createdAt = createdAt.Add(timeOffset.Value);
            createdAt = RoundTime(createdAt);

            return new Calculation(Guid.CreateVersion7(), expression, new User(user ?? "test_user"), createdAt, createdAt, status ?? CalculationStatus.Pending);
        }
        private static Calculation CreateCalculationPending(string expression = "1 + 2", string? user = null, TimeSpan? timeOffset = null)
        {
            return CreateCalculation(expression, CalculationStatus.Pending, user, timeOffset);
        }
        private static Calculation CreateCalculationInProgress(string expression = "1 + 2", string? user = null, TimeSpan? timeOffset = null)
        {
            return CreateCalculation(expression, CalculationStatus.InProgress, user, timeOffset);
        }
        private static Calculation CreateCalculationSuccess(string expression = "1 + 2", string? user = null, double result = 3, TimeSpan? timeOffset = null)
        {
            return CreateCalculation(expression, CalculationStatus.CreateSuccess(result), user, timeOffset);
        }
        private static Calculation CreateCalculationFailed(string expression = "1 + 2", string? user = null, TimeSpan? timeOffset = null)
        {
            return CreateCalculation(expression,
                CalculationStatus.CreateFailed(
                    Entities.Enums.CalculationErrorCode.ArithmeticError,
                    new CalculationErrorDetails()
                    { 
                        ErrorCode = CalculationErrorDetails.DivisionByZeroErrorCode,
                        Offset = 10,
                        Length = 1
                    }), user, timeOffset);

        }
        private static Calculation CreateCalculationCancelled(string expression = "1 + 2", string? user = null, string cancelledBy = "test_user2", TimeSpan? timeOffset = null)
        {
            return CreateCalculation(expression, CalculationStatus.CreateCancelled(new User(cancelledBy)), user, timeOffset);
        }

        private static IEnumerable<Calculation> ApplyFilter(IEnumerable<Calculation> calc, CalculationFilters filter)
        {
            return calc.Where(o =>
                (filter.State == null || o.Status.State == filter.State.Value) &&
                (filter.Id == null || o.Id == filter.Id.Value) &&
                (filter.CreatedAtMin == null || o.CreatedAt >= filter.CreatedAtMin) &&
                (filter.CreatedAtMax == null || o.CreatedAt < filter.CreatedAtMax) &&
                (filter.UpdatedAtMin == null || o.UpdatedAt >= filter.UpdatedAtMin.Value) &&
                (filter.UpdatedAtMax == null || o.UpdatedAt < filter.UpdatedAtMax.Value) &&
                (filter.CreatedBy == null || o.CreatedBy.Login == filter.CreatedBy) &&
                (filter.Expression == null || o.Expression.Contains(filter.Expression))
                );
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


            var paginatedList = await controller.GetCalculationsListAsync(CalculationFilters.NoFilters, PaginationParams.AllData, deadlockProtection.Token);
            var list = paginatedList.Items;
            list.Reverse();

            Assert.All(insertionList.Zip(list), (item) =>
            {
                Assert.Equal(item.First.Id, item.Second.Id);
                Assert.Equal(item.First.Status.State, item.Second.Status.State);

                Assert.Equivalent(item.First, item.Second);
            });
        }


        [Fact]
        public async Task GetCalculationsListWithFiltersTest()
        {
            CancellationTokenSource deadlockProtection = new CancellationTokenSource();
            deadlockProtection.CancelAfter(TimeSpan.FromSeconds(60));

            using var tempDirHolder = new TempDirectoryHolder();
            await using var controller = CreateController(tempDirHolder.Directory);
            await controller.Init(deadlockProtection.Token);

            List<Calculation> insertionList = [
                CreateCalculationPending(user: "abc"),
                CreateCalculationInProgress(user: "bcd", timeOffset: TimeSpan.FromMilliseconds(10)),
                CreateCalculationSuccess(user: "bcd", timeOffset: TimeSpan.FromMilliseconds(20)),
                CreateCalculationFailed(user: "abc", expression: "log(10) / cos(1)", timeOffset: TimeSpan.FromMilliseconds(30)),
                CreateCalculationCancelled(user: "abc", timeOffset: TimeSpan.FromMilliseconds(40))
            ];

            foreach (var item in insertionList)
            {
                await controller.AddCalculationAsync(item, deadlockProtection.Token);
            }


            List<CalculationFilters> filters = [
                new CalculationFilters() { State = Entities.Enums.CalculationState.InProgress },
                new CalculationFilters() { CreatedBy = "abc" },
                new CalculationFilters() { Expression = "cos" },
                new CalculationFilters() { Id = insertionList[0].Id },
                new CalculationFilters() { CreatedAtMin = insertionList[1].CreatedAt, CreatedAtMax = insertionList[2].CreatedAt.AddMilliseconds(2) },
                new CalculationFilters() { UpdatedAtMin = insertionList[1].CreatedAt, UpdatedAtMax = insertionList[2].CreatedAt.AddMilliseconds(2) },
                new CalculationFilters() { UpdatedAtMin = insertionList[1].CreatedAt, CreatedBy = "abc" },
            ];

            foreach (var filter in filters)
            {
                var paginatedList = await controller.GetCalculationsListAsync(filter, PaginationParams.AllData, deadlockProtection.Token);
                paginatedList.Items.Reverse();
                Assert.Equivalent(ApplyFilter(insertionList, filter), paginatedList.Items);
            }
        }

        [Fact]
        public async Task GetCalculationsListWithPaginationTest()
        {
            CancellationTokenSource deadlockProtection = new CancellationTokenSource();
            deadlockProtection.CancelAfter(TimeSpan.FromSeconds(60));

            using var tempDirHolder = new TempDirectoryHolder();
            await using var controller = CreateController(tempDirHolder.Directory);
            await controller.Init(deadlockProtection.Token);

            List<Calculation> insertionList = [
                CreateCalculationPending(user: "abc"),
                CreateCalculationInProgress(user: "bcd", timeOffset: TimeSpan.FromMilliseconds(10)),
                CreateCalculationSuccess(user: "bcd", timeOffset: TimeSpan.FromMilliseconds(20)),
                CreateCalculationFailed(user: "abc", expression: "log(10) / cos(1)", timeOffset: TimeSpan.FromMilliseconds(30)),
                CreateCalculationCancelled(user: "abc", timeOffset: TimeSpan.FromMilliseconds(40))
            ];

            foreach (var item in insertionList)
            {
                await controller.AddCalculationAsync(item, deadlockProtection.Token);
            }

            var paginatedList = await controller.GetCalculationsListAsync(CalculationFilters.NoFilters, new PaginationParams(0, 2), deadlockProtection.Token);
            Assert.Equal(0u, paginatedList.Offset);
            Assert.Equal(2u, paginatedList.Limit);
            Assert.Equal(2, paginatedList.Items.Count);
            Assert.Null(paginatedList.TotalItemsCount);
            Assert.Equal(insertionList[insertionList.Count - 1].Id, paginatedList.Items[0].Id);
            Assert.Equal(insertionList[insertionList.Count - 2].Id, paginatedList.Items[1].Id);

            paginatedList = await controller.GetCalculationsListAsync(CalculationFilters.NoFilters, new PaginationParams(2, 2, true), deadlockProtection.Token);
            Assert.Equal(2u, paginatedList.Offset);
            Assert.Equal(2u, paginatedList.Limit);
            Assert.Equal(2, paginatedList.Items.Count);
            Assert.NotNull(paginatedList.TotalItemsCount);
            Assert.Equal(5u, paginatedList.TotalItemsCount.Value);
            Assert.Equal(insertionList[insertionList.Count - 3].Id, paginatedList.Items[0].Id);
            Assert.Equal(insertionList[insertionList.Count - 4].Id, paginatedList.Items[1].Id);
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
        public async Task ResetNonFinalStateToPendingTest()
        {
            CancellationTokenSource deadlockProtection = new CancellationTokenSource();
            deadlockProtection.CancelAfter(TimeSpan.FromSeconds(60));

            using var tempDirHolder = new TempDirectoryHolder();
            await using var controller = CreateController(tempDirHolder.Directory);
            await controller.Init(deadlockProtection.Token);

            List<Calculation> insertionList = [
                CreateCalculationPending(user: "abc"),
                CreateCalculationInProgress(user: "bcd", timeOffset: TimeSpan.FromMilliseconds(10)),
                CreateCalculationSuccess(user: "bcd", timeOffset: TimeSpan.FromMilliseconds(20)),
                CreateCalculationFailed(user: "abc", expression: "log(10) / cos(1)", timeOffset: TimeSpan.FromMilliseconds(30)),
                CreateCalculationCancelled(user: "abc", timeOffset: TimeSpan.FromMilliseconds(40))
            ];

            foreach (var item in insertionList)
            {
                await controller.AddCalculationAsync(item, deadlockProtection.Token);
            }

            DateTime newUpdateTime = RoundTime(DateTime.UtcNow);

            var pendingCheck = await controller.GetCalculationsListAsync(new CalculationFilters() { State = Entities.Enums.CalculationState.Pending }, PaginationParams.AllData, deadlockProtection.Token);
            Assert.Single(pendingCheck.Items);

            int resetedCount = await controller.ResetNonFinalStateToPendingAsync(insertionList[0].CreatedAt, newUpdateTime, deadlockProtection.Token);
            Assert.Equal(0, resetedCount);

            pendingCheck = await controller.GetCalculationsListAsync(new CalculationFilters() { State = Entities.Enums.CalculationState.Pending }, PaginationParams.AllData, deadlockProtection.Token);
            Assert.Single(pendingCheck.Items);

            // Second try when time covers all records

            resetedCount = await controller.ResetNonFinalStateToPendingAsync(insertionList[insertionList.Count - 1].CreatedAt.AddMilliseconds(100), newUpdateTime, deadlockProtection.Token);
            Assert.Equal(1, resetedCount);

            pendingCheck = await controller.GetCalculationsListAsync(new CalculationFilters() { State = Entities.Enums.CalculationState.Pending }, PaginationParams.AllData, deadlockProtection.Token);
            Assert.Equal(2, pendingCheck.Items.Count);
        }


        [Fact]
        public async Task DeleteCalculationsTest()
        {
            CancellationTokenSource deadlockProtection = new CancellationTokenSource();
            deadlockProtection.CancelAfter(TimeSpan.FromSeconds(60));

            using var tempDirHolder = new TempDirectoryHolder();
            await using var controller = CreateController(tempDirHolder.Directory);
            await controller.Init(deadlockProtection.Token);

            List<Calculation> insertionList = [
                CreateCalculationPending(user: "abc"),
                CreateCalculationInProgress(user: "bcd", timeOffset: TimeSpan.FromMilliseconds(10)),
                CreateCalculationSuccess(user: "bcd", timeOffset: TimeSpan.FromMilliseconds(20)),
                CreateCalculationFailed(user: "abc", expression: "log(10) / cos(1)", timeOffset: TimeSpan.FromMilliseconds(30)),
                CreateCalculationCancelled(user: "abc", timeOffset: TimeSpan.FromMilliseconds(40))
            ];

            foreach (var item in insertionList)
            {
                await controller.AddCalculationAsync(item, deadlockProtection.Token);
            }

            DateTime newUpdateTime = RoundTime(DateTime.UtcNow);


            int deletedCount = await controller.DeleteCalculationsAsync(insertionList[0].CreatedAt.AddMilliseconds(1), deadlockProtection.Token);
            Assert.Equal(1, deletedCount);

            var remainingCheck = await controller.GetCalculationsListAsync(CalculationFilters.NoFilters, PaginationParams.AllData, deadlockProtection.Token);
            Assert.Equal(insertionList.Count - 1, remainingCheck.Items.Count);

            // Remove more

            deletedCount = await controller.DeleteCalculationsAsync(insertionList[insertionList.Count - 1].CreatedAt.AddMilliseconds(-1), deadlockProtection.Token);
            Assert.Equal(insertionList.Count - 2, deletedCount);

            remainingCheck = await controller.GetCalculationsListAsync(CalculationFilters.NoFilters, PaginationParams.AllData, deadlockProtection.Token);
            Assert.Single(remainingCheck.Items);
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
                await controller.GetCalculationsListAsync(CalculationFilters.NoFilters, PaginationParams.AllData, deadlockProtection.Token);
                reads++;
            }

            await bcgTask;

            _output.WriteLine($"Reads = {reads}");

            var finalList = await controller.GetCalculationsListAsync(CalculationFilters.NoFilters, PaginationParams.AllData, deadlockProtection.Token);
            Assert.Equal(100, finalList.Items.Count);
        }
    }
}
