using ExprCalc.ExpressionParsing.Parser;
using ExprCalc.ExpressionParsing.Representation.AstNodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.ExpressionParsing.Representation
{
    public static class MathExpression
    {
        /// <summary>
        /// Runs expression validation. If expression is invalid, throws <see cref="ExpressionParserException"/> or its subtypes
        /// </summary>
        /// <param name="expression">Math expression</param>
        /// <param name="validateNumbersCanBeRepresentedAsDouble">If true then additionally validates that the number is not too large to be represented as double</param>
        public static void ValidateExpression(string expression, bool validateNumbersCanBeRepresentedAsDouble = false)
        {
            ExpressionParser.ParseExpression< ValidationExpressionNodesFactory, EmptyNode>(expression, new ValidationExpressionNodesFactory(validateNumbersCanBeRepresentedAsDouble));
        }

        /// <summary>
        /// Runs expression validation. If expression is invalid, throws <see cref="ExpressionParserException"/> or its subtypes
        /// </summary>
        /// <param name="expression">Math expression</param>
        /// <param name="validateNumbersCanBeRepresentedAsDouble">If true then additionally validates that the number is not too large to be represented as double</param>
        internal static async ValueTask ValidateExpressionAsync(string expression, bool validateNumbersCanBeRepresentedAsDouble = false, CancellationToken cancellationToken = default)
        {
            await ExpressionParser.ParseExpressionAsync<ValidationExpressionNodesFactory, EmptyNode>(expression, new ValidationExpressionNodesFactory(validateNumbersCanBeRepresentedAsDouble), cancellationToken);
        }

        /// <summary>
        /// Calculates math expression
        /// </summary>
        /// <param name="expression">Math expression</param>
        /// <param name="numberValidationBehaviour">Number validation behaviour</param>
        /// <returns>Calculated value</returns>
        public static double CalculateExpression(string expression, NumberValidationBehaviour numberValidationBehaviour = NumberValidationBehaviour.Strict)
        {
            return ExpressionParser.ParseExpression<CalculationExpressionNodesFactory, double>(expression, new CalculationExpressionNodesFactory(numberValidationBehaviour));
        }

        /// <summary>
        /// Calculates math expression asynchronously
        /// </summary>
        /// <param name="expression">Math expression</param>
        /// <param name="numberValidationBehaviour">Number validation behaviour</param>
        /// <returns>Calculated value</returns>
        public static ValueTask<double> CalculateExpressionAsync(string expression, NumberValidationBehaviour numberValidationBehaviour = NumberValidationBehaviour.Strict, CancellationToken cancellationToken = default)
        {
            return ExpressionParser.ParseExpressionAsync<CalculationExpressionNodesFactory, double>(expression, new CalculationExpressionNodesFactory(numberValidationBehaviour), cancellationToken);
        }

        /// <summary>
        /// Builds AST for expression
        /// </summary>
        /// <param name="expression">Math expression</param>
        /// <returns>Expression AST</returns>
        public static ExpressionNode BuildExpressionAst(string expression)
        {
            return ExpressionParser.ParseExpression<AstBuildingExpressionNodesFactory, ExpressionNode>(expression, new AstBuildingExpressionNodesFactory());
        }
    }
}
