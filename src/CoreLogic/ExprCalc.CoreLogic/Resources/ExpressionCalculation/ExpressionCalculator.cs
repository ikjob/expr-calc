using ExprCalc.Entities;
using ExprCalc.Entities.Enums;
using ExprCalc.ExpressionParsing.Parser;
using ExprCalc.ExpressionParsing.Representation;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.CoreLogic.Resources.ExpressionCalculation
{
    internal class ExpressionCalculator : IExpressionCalculator
    {
        private readonly IExternalCalculationStatusUpdater _externalStatusUpdater;
        private readonly ILogger<ExpressionCalculator> _logger;

        public ExpressionCalculator(
            IExternalCalculationStatusUpdater externalStatusUpdater,
            ILogger<ExpressionCalculator> logger)
        {
            _externalStatusUpdater = externalStatusUpdater;
            _logger = logger;
        }


        private static FailedCalculationStatus CreateFailedStatusFromExpressionParserException(ExpressionParserException parserExc)
        {
            var errorCode = parserExc switch
            {
                InvalidLexemaException => CalculationErrorDetails.InvalidLexemaErrorCode,
                UnknownIdentifierException => CalculationErrorDetails.UnknownFuntionOrIdentifierErrorCode,
                UnbalancedExpressionException => CalculationErrorDetails.UnbalancedExpressionErrorCode,
                _ => CalculationErrorDetails.BadExpressionSyntaxErrorCode
            };

            return CalculationStatus.CreateFailed(CalculationErrorCode.BadExpressionSyntax, new CalculationErrorDetails()
            {
                ErrorCode = errorCode,
                Offset = parserExc.Offset,
                Length = parserExc.Length
            });
        }
        private static FailedCalculationStatus CreateFailedStatusFromExpressionCalculationException(ExpressionCalculationException calcExc)
        {
            var errorCode = calcExc.ErrorType switch
            {
                ExpressionCalculationErrorType.Overflow => CalculationErrorDetails.OverflowErrorCode,
                ExpressionCalculationErrorType.NumberTooLarge => CalculationErrorDetails.NumberToLargeErrorCode,
                ExpressionCalculationErrorType.DivisionByZero => CalculationErrorDetails.DivisionByZeroErrorCode,
                ExpressionCalculationErrorType.LnFromNegative => CalculationErrorDetails.LnFromNegativeErrorCode,
                ExpressionCalculationErrorType.PowZeroZero => CalculationErrorDetails.PowZeroZeroErrorCode,
                ExpressionCalculationErrorType.NegativeBaseFractionalExponent => CalculationErrorDetails.NegativeBaseFractionalExponentErrorCode,
                _ => CalculationErrorDetails.UnspecifiedArithmeticProblemErrorCode
            };

            return CalculationStatus.CreateFailed(CalculationErrorCode.ArithmeticError, new CalculationErrorDetails()
            {
                ErrorCode = errorCode,
                Offset = calcExc.Offset,
                Length = calcExc.Length
            });
        }

        /// <summary>
        /// Performes expression syntax validation
        /// </summary>
        /// <returns>New status if the change is required</returns>
        private static FailedCalculationStatus? ValidateExpression(Calculation calculation)
        {
            try
            {
                MathExpression.ValidateExpression(calculation.Expression, validateNumbersCanBeRepresentedAsDouble: false);
                return null;
            }
            catch (ExpressionParserException parserExc)
            {
                return CreateFailedStatusFromExpressionParserException(parserExc);
            }
        }

        /// <summary>
        /// Runs actual expression calculation
        /// </summary>
        private static async Task<CalculationStatus> CalculateExpression(Calculation calculation, CancellationToken token)
        {
            try
            {
                // TODO: introduce delays
                double result = await MathExpression.CalculateExpressionAsync(calculation.Expression, NumberValidationBehaviour.Strict);
                return CalculationStatus.CreateSuccess(result);
            }
            catch (ExpressionParserException parserExc)
            {
                return CreateFailedStatusFromExpressionParserException(parserExc);
            }
            catch (ExpressionCalculationException calculationExc)
            {
                return CreateFailedStatusFromExpressionCalculationException(calculationExc);
            }
        }


        /// <summary>
        /// Runs expression calculation on <paramref name="calculation"/> and set results into its status
        /// </summary>
        /// <param name="calculation">Source calculation</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Status, that is exactly the <paramref name="calculation"/> status</returns>
        /// <remarks>
        /// Do calculation in two parsing phases:
        /// <list type="number">
        /// <item>Run expression syntax validation</item>
        /// <item>Run calculation on the fly</item>
        /// </list>
        /// <para />
        /// This is the most optimal way to do that. 
        /// We have 3 possible ways to do syntax validation and calculation:
        /// <list type="number">
        /// <item>
        /// Calculate on the fly and stop on the first error.
        /// The problem here is that the first error can be a mathematical error and it will be shown to the user.
        /// Example: <code>(1 / 0) * 2 + invalid;;;;;</code>
        /// Execution in example will stop at the division by zero error, ignoring invalid syntax later.
        /// </item>
        /// <item>
        /// Parse the expression into intermediate representation and do the validation during this pass.
        /// Then calculate expression from this intermediate representation.
        /// Possible representations: AST and RPN.
        /// In both cases memory allocations is required for every expression node.
        /// </item>
        /// <item>
        /// Run validation pass, then run calculation on the fly.
        /// In this case no significant amount of allocations is needed, because expression nodes collapses on the fly
        /// </item>
        /// </list>
        /// </remarks>
        public async Task<CalculationStatus> Calculate(Calculation calculation, CancellationToken softCancellationToken, CancellationToken hardCancellationToken)
        {
            var state = calculation.Status.State;
            if (state != CalculationState.Pending && state != CalculationState.Cancelled)
                throw new InvalidOperationException($"Calculation cannot be run, because it is in unxpected state: {state}");

            softCancellationToken.ThrowIfCancellationRequested();
            hardCancellationToken.ThrowIfCancellationRequested();

            if (!calculation.TryChangeStatus(CalculationStatus.InProgress, out var prevStatus))
            {
                if (prevStatus.State != CalculationState.Cancelled)
                    _logger.LogWarning("Calculation item has unexpected state at the begging of calculation. Id = {id}, State = {state}", calculation.Id, prevStatus.State);

                return calculation.Status;
            }
            await _externalStatusUpdater.UpdateStatus(calculation, hardCancellationToken);

            var statusFromValidation = ValidateExpression(calculation);
            if (statusFromValidation != null)
            {
                if (!calculation.TryChangeStatus(statusFromValidation, out prevStatus))
                {
                    if (prevStatus.State != CalculationState.Cancelled)
                        _logger.LogWarning("Unable to set validation result. Calculation item has unexpected state. Id = {id}, State = {state}", calculation.Id, prevStatus.State);

                    // If status was changed by outsied then just return
                    return calculation.Status;
                }
                // Update status in storage
                await _externalStatusUpdater.UpdateStatus(calculation, hardCancellationToken);
                return calculation.Status;
            }


            softCancellationToken.ThrowIfCancellationRequested();
            hardCancellationToken.ThrowIfCancellationRequested();

            using var linkedCancellationSource = CancellationTokenSource.CreateLinkedTokenSource(softCancellationToken, hardCancellationToken);

            var calculationResultStatus = await CalculateExpression(calculation, linkedCancellationSource.Token);
            if (!calculation.TryChangeStatus(calculationResultStatus, out prevStatus))
            {
                if (prevStatus.State != CalculationState.Cancelled)
                    _logger.LogWarning("Unable to set calculation result. Calculation item has unexpected state. Id = {id}, State = {state}", calculation.Id, prevStatus.State);

                // If status was changed by outsied then just return
                return calculation.Status;
            }
            // Update status in storage
            await _externalStatusUpdater.UpdateStatus(calculation, hardCancellationToken);
            return calculation.Status;
        }
    }
}
