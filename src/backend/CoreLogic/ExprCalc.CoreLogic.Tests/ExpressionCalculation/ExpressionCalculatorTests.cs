using ExprCalc.CoreLogic.Resources.ExpressionCalculation;
using ExprCalc.Entities;
using ExprCalc.Entities.Enums;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.CoreLogic.Tests.ExpressionCalculation
{
    public class ExpressionCalculatorTests
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

        private static Calculation CreateCalculation(string expression)
        {
            return Calculation.CreateInitial(expression, new User("test_user"));
        }


        // ================


        [Theory]
        [InlineData("1 + 2", 3.0)]
        [InlineData("200 / 2.0 + 2 ^ 2", 104.0)]
        [InlineData("-15 - 10", -25.0)]
        public async Task CorrectCalculationTest(string expr, double expectedResult)
        {
            using CancellationTokenSource deadlockProtection = new CancellationTokenSource();
            deadlockProtection.CancelAfter(TimeSpan.FromSeconds(60));

            var (calculator, updater) = CreateCalculator();

            var calculation = CreateCalculation(expr);
            var resultStatus = await calculator.Calculate(calculation, deadlockProtection.Token, deadlockProtection.Token);

            Assert.Equal(Entities.Enums.CalculationState.Success, resultStatus.State);
            Assert.Equal(Entities.Enums.CalculationState.Success, calculation.Status.State);
            Assert.Equal(calculation.Status, resultStatus);

            Assert.True(updater.Updates.TryGetValue(calculation.Id, out var submittedCalculation));
            Assert.Equal(calculation, submittedCalculation);

            Assert.True(calculation.Status.IsSuccess(out var successStatus));
            Assert.Equal(expectedResult, successStatus.CalculationResult, 1E-8);
        }


        [Theory]
        [InlineData("1#10", CalculationErrorCode.BadExpressionSyntax, CalculationErrorDetails.InvalidLexemaErrorCode)]
        [InlineData("1 + ", CalculationErrorCode.BadExpressionSyntax, CalculationErrorDetails.UnbalancedExpressionErrorCode)]
        [InlineData("cos(3)", CalculationErrorCode.BadExpressionSyntax, CalculationErrorDetails.UnknownFuntionOrIdentifierErrorCode)]
        [InlineData("10 / 0.0", CalculationErrorCode.ArithmeticError, CalculationErrorDetails.DivisionByZeroErrorCode)]
        [InlineData("ln(-1)", CalculationErrorCode.ArithmeticError, CalculationErrorDetails.LnFromNegativeErrorCode)]
        [InlineData("1e99999999999999999", CalculationErrorCode.ArithmeticError, CalculationErrorDetails.NumberToLargeErrorCode)]
        public async Task BadCalculationTest(string expr, CalculationErrorCode errorCode, string detailedErrorCode)
        {
            using CancellationTokenSource deadlockProtection = new CancellationTokenSource();
            deadlockProtection.CancelAfter(TimeSpan.FromSeconds(60));

            var (calculator, updater) = CreateCalculator();

            var calculation = CreateCalculation(expr);
            var resultStatus = await calculator.Calculate(calculation, deadlockProtection.Token, deadlockProtection.Token);

            Assert.Equal(Entities.Enums.CalculationState.Failed, resultStatus.State);
            Assert.Equal(Entities.Enums.CalculationState.Failed, calculation.Status.State);
            Assert.Equal(calculation.Status, resultStatus);

            Assert.True(updater.Updates.TryGetValue(calculation.Id, out var submittedCalculation));
            Assert.Equal(calculation, submittedCalculation);

            Assert.True(calculation.Status.IsFailed(out var failedStatus));
            Assert.Equal(errorCode, failedStatus.ErrorCode);
            Assert.Equal(detailedErrorCode, failedStatus.ErrorDetails.ErrorCode);
        }


        [Fact]
        public async Task CalculationCancellationTest()
        {
            using CancellationTokenSource deadlockProtection = new CancellationTokenSource();
            deadlockProtection.CancelAfter(TimeSpan.FromSeconds(60));

            using CancellationTokenSource cancellationSource = new CancellationTokenSource();

            var (calculator, updater) = CreateCalculator(
                new Dictionary<ExpressionParsing.Parser.ExpressionOperationType, TimeSpan>()
                {
                    { ExpressionParsing.Parser.ExpressionOperationType.Add, TimeSpan.FromMilliseconds(500) }
                });

            var calculation = CreateCalculation("1 + 2 + 3 + 4 + 5 + 6");
            
            Func<Task<CalculationStatus?>> runBackground = async () =>
            {
                await Task.Yield();
                try
                {
                    return await calculator.Calculate(calculation, cancellationSource.Token, deadlockProtection.Token);
                }
                catch (OperationCanceledException)
                {
                }
                return null;
            };


            var bckgCalc = runBackground();

            await Task.Delay(100);
            cancellationSource.Cancel();

            await bckgCalc;
            Assert.True(calculation.Status.IsPending() || calculation.Status.IsInProgress());

            if (!updater.Updates.IsEmpty)
            {
                Assert.True(updater.Updates.TryGetValue(calculation.Id, out var updatedCalc));
                Assert.Equal(calculation, updatedCalc);
                Assert.True(updatedCalc.Status.IsPending() || updatedCalc.Status.IsInProgress());
            }
        }
    }
}
