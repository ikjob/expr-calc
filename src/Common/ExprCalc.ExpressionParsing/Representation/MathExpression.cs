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
        public static void ValidateExpression(string expression)
        {
            ExpressionParser.SharedParser.ParseExpression< ValidationExpressionNodesFactory, EmptyNode>(expression, new ValidationExpressionNodesFactory());
        }

        /// <summary>
        /// Calculates math expression
        /// </summary>
        /// <param name="expression">Math expression</param>
        /// <param name="numberValidationBehaviour">Number validation behaviour</param>
        /// <returns>Calculated value</returns>
        public static double CalculateExpression(string expression, NumberValidationBehaviour numberValidationBehaviour = NumberValidationBehaviour.Strict)
        {
            return ExpressionParser.SharedParser.ParseExpression<CalculationExpressionNodesFactory, double>(expression, new CalculationExpressionNodesFactory(numberValidationBehaviour));
        }

        /// <summary>
        /// Builds AST for expression
        /// </summary>
        /// <param name="expression">Math expression</param>
        /// <returns>Expression AST</returns>
        public static ExpressionNode BuildExpressionAst(string expression)
        {
            return ExpressionParser.SharedParser.ParseExpression<AstBuildingExpressionNodesFactory, ExpressionNode>(expression, new AstBuildingExpressionNodesFactory());
        }
    }
}
