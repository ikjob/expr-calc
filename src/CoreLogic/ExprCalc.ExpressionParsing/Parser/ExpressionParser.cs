using ExprCalc.ExpressionParsing.Lexer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.ExpressionParsing.Parser
{
    public static class ExpressionParser
    {
        private const int MaxTokenDebugInfoLength = 16;
        private const int TokenDebugInfoBackwardOffset = 4;

        private record struct ExpressionOperationExt(ExpressionOperation Op, int Offset);

        private enum TrackingState
        {
            OperatorExpected,
            OperandExpected
        }

        // =============

        private static ReadOnlySpan<char> GetTextAroundOffset(string expression, int offset)
        {
            if (offset < 0 || offset >= expression.Length)
                return "";

            int startPos = Math.Max(0, offset - TokenDebugInfoBackwardOffset);
            if (startPos + MaxTokenDebugInfoLength <= expression.Length)
                return expression.AsSpan(startPos, MaxTokenDebugInfoLength);
            return expression.AsSpan(startPos);
        }


        /// <summary>
        /// Common function to parse number as double
        /// </summary>
        /// <param name="numberText">Number text</param>
        /// <param name="offsetInExpression">Offset inside expression. Used to generate proper exception</param>
        /// <param name="allowInfNaN">Allows Inf value (by default is not allowed)</param>
        /// <returns>Parsed value</returns>
        /// <exception cref="InvalidNumberException">Parsing error occured</exception>
        public static double ParseNumberAsDouble(ReadOnlySpan<char> numberText, int offsetInExpression, bool allowInf = false)
        {
            double result;
            try
            {
                result = double.Parse(numberText, CultureInfo.InvariantCulture);
            }
            catch (FormatException fmtExc)
            {
                // If the number came from Lexer then this exception cannot happen
                throw new InvalidNumberException($"Found number with incorrect format. Offset = {offsetInExpression}. Value = '{numberText.Slice(0, Math.Min(numberText.Length, MaxTokenDebugInfoLength))}{(numberText.Length <= MaxTokenDebugInfoLength ? "" : "..")}'", offsetInExpression, numberText.Length, fmtExc);
            }
            catch (OverflowException ovfExc)
            {
                // Should never happen in the modern versions of .NET
                throw new InvalidNumberException($"Found number which is too large to be parsed. Offset = {offsetInExpression}. Value = '{numberText.Slice(0, Math.Min(numberText.Length, MaxTokenDebugInfoLength))}{(numberText.Length <= MaxTokenDebugInfoLength ? "" : "..")}'", offsetInExpression, numberText.Length, ovfExc);
            }

            if (!allowInf && double.IsInfinity(result))
                throw new InvalidNumberException($"Found number which is too large to be parsed. Offset = {offsetInExpression}. Value = '{numberText.Slice(0, Math.Min(numberText.Length, MaxTokenDebugInfoLength))}{(numberText.Length <= MaxTokenDebugInfoLength ? "" : "..")}'", offsetInExpression, numberText.Length);

            return result;
        }


        private static ExpressionOperationExt GetFunctionNameForIdentifier(in Token token)
        {
            Debug.Assert(token.Type == TokenType.Identifier);

            var identifier = token.GetTokenText();
            foreach (var func in ExpressionOperation.Functions)
            {
                if (identifier.Equals(func.Name, StringComparison.OrdinalIgnoreCase))
                    return new ExpressionOperationExt(func, token.Offset);
            }

            throw new UnknownIdentifierException($"Found unknown identifier. Offset = {token.Offset}. Value = '{token.GetTokenTextDebug()}'", token.Offset, token.Length);
        }

        private static TNode ApplyOperator<TNodeFactory, TNode>(TNodeFactory nodeFactory, Stack<TNode> args, ExpressionOperationExt oper, string expression)
            where TNodeFactory : IExpressionNodesFactory<TNode>
        {
            Debug.Assert(oper.Op.OperationType != null);

            switch (oper.Op.OperandCount)
            {
                case 1:
                    if (args.Count < 1)
                        throw new UnbalancedExpressionException($"Operation {oper.Op.OperationType} expected 1 operand which is not provided. Offset = {oper.Offset}. Context = {GetTextAroundOffset(expression, oper.Offset)}", 0);
                    var lastArg = args.Pop();
                    return nodeFactory.UnaryOp(oper.Op.OperationType.Value, oper.Offset, lastArg);
                case 2:
                    if (args.Count < 2)
                        throw new UnbalancedExpressionException($"Operation {oper.Op.OperationType} expected 2 operands which is not provided. Offset = {oper.Offset}. Context = {GetTextAroundOffset(expression, oper.Offset)}", 0);
                    var arg2 = args.Pop();
                    var arg1 = args.Pop();
                    return nodeFactory.BinaryOp(oper.Op.OperationType.Value, oper.Offset, arg1, arg2);
                default:
                    throw new UncatchableParserException("Unsupported number of operands: " + oper.Op.OperandCount.ToString());
            }
        }

        /// <summary>
        /// Parses math expression using modified Shunting Yard algorithm
        /// </summary>
        /// <param name="expression">String with expression</param>
        /// <param name="nodeFactory">Factory to create nodes</param>
        /// <returns></returns>
        /// <exception cref="InvalidLexemaException">Invalid lexema found</exception>
        /// <exception cref="InvalidExpressionException">General problems with passed expression</exception>
        /// <exception cref="UnbalancedExpressionException">Opening closing braces mimatch, or operator arguments mismatch</exception>
        /// <exception cref="InvalidNumberException">Found number that cannot be parsed</exception>
        /// <exception cref="UnknownIdentifierException">Found unknown function identifier</exception>
        public static TNode ParseExpression<TNodeFactory, TNode>(string expression, TNodeFactory nodeFactory)
            where TNodeFactory : IExpressionNodesFactory<TNode>
        {
            Stack<TNode> outputNodes = new Stack<TNode>();
            Stack<ExpressionOperationExt> operatorStack = new Stack<ExpressionOperationExt>();
            TrackingState trackingState = TrackingState.OperandExpected;

            foreach (var token in TokenStream.EnumerateTokens(expression, allowErrors: true))
            {
                if (token.IsError)
                    throw new InvalidLexemaException($"{token.ErrorDescription}. Offset = {token.Offset}. Value = '{token.GetTokenTextDebug()}'", token.Offset, token.Length);

                ExpressionOperationExt operatorFromStack;

                // Require brackets after function
                if (operatorStack.TryPeek(out operatorFromStack) &&
                    operatorFromStack.Op.IsFunction &&
                    token.Type != TokenType.OpeningBracket)
                {
                    throw new InvalidExpressionException($"Opening bracket expected after the function name, but found {token.Type}. Offset = {token.Offset}. Context = '{GetTextAroundOffset(expression, token.Offset)}'", token.Offset);
                }

                switch (token.Type)
                {
                    case TokenType.Number:
                        if (trackingState != TrackingState.OperandExpected)
                            throw new InvalidExpressionException($"Operator expected, but number is found. Offset = {token.Offset}. Context = '{GetTextAroundOffset(expression, token.Offset)}'", token.Offset);

                        var node = nodeFactory.Number(token.GetTokenText(), token.Offset);
                        outputNodes.Push(node);
                        break;
                    case TokenType.Identifier:
                        if (trackingState != TrackingState.OperandExpected)
                            throw new InvalidExpressionException($"Operator expected, but function call is found. Offset = {token.Offset}. Context = '{GetTextAroundOffset(expression, token.Offset)}'", token.Offset);

                        var func = GetFunctionNameForIdentifier(token);
                        operatorStack.Push(func);
                        break;
                    case TokenType.Plus when trackingState == TrackingState.OperandExpected:
                    case TokenType.Minus when trackingState == TrackingState.OperandExpected:
                        var newUnaryOperator = ExpressionOperation.GetUnaryOperatorForLexerToken(token.Type);
                        operatorStack.Push(new ExpressionOperationExt(newUnaryOperator, token.Offset));
                        break;
                    case TokenType.Plus:
                    case TokenType.Minus:
                    case TokenType.MultiplicationSign:
                    case TokenType.DivisionSign:
                    case TokenType.ExponentSign:
                        if (trackingState != TrackingState.OperatorExpected)
                            throw new InvalidExpressionException($"Unexpected operator sequence. Offset = {token.Offset}. Context = '{GetTextAroundOffset(expression, token.Offset)}'", token.Offset);

                        var newOperator = ExpressionOperation.GetOperatorForLexerToken(token.Type);
                        while (operatorStack.TryPeek(out operatorFromStack) &&
                            operatorFromStack.Op != ExpressionOperation.OpeningBracket &&
                            (operatorFromStack.Op.Priority > newOperator.Priority || (operatorFromStack.Op.Priority == newOperator.Priority && newOperator.Associativity == OperatorAssociativity.Left)))
                        {
                            operatorFromStack = operatorStack.Pop();
                            outputNodes.Push(ApplyOperator(nodeFactory, outputNodes, operatorFromStack, expression));
                        }
                        operatorStack.Push(new ExpressionOperationExt(newOperator, token.Offset));
                        break;
                    case TokenType.OpeningBracket:
                        if (trackingState != TrackingState.OperandExpected)
                            throw new InvalidExpressionException($"Unexpected opening bracket. Offset = {token.Offset}. Context = '{GetTextAroundOffset(expression, token.Offset)}'", token.Offset);

                        operatorStack.Push(new ExpressionOperationExt(ExpressionOperation.OpeningBracket, token.Offset));
                        break;
                    case TokenType.ClosingBracket:
                        if (trackingState != TrackingState.OperatorExpected)
                            throw new InvalidExpressionException($"Unexpected closing bracket. Offset = {token.Offset}. Context = '{GetTextAroundOffset(expression, token.Offset)}'", token.Offset);

                        while (operatorStack.TryPeek(out operatorFromStack) &&
                               operatorFromStack.Op != ExpressionOperation.OpeningBracket)
                        {
                            operatorFromStack = operatorStack.Pop();
                            outputNodes.Push(ApplyOperator(nodeFactory, outputNodes, operatorFromStack, expression));
                        }

                        if (!operatorStack.TryPeek(out operatorFromStack) || operatorFromStack.Op != ExpressionOperation.OpeningBracket)
                            throw new UnbalancedExpressionException($"Closing bracket without paired opening bracket found. Offset = {token.Offset}. Context = '{GetTextAroundOffset(expression, token.Offset)}'", token.Offset);
                        operatorStack.Pop();

                        // Check function
                        if (operatorStack.TryPeek(out operatorFromStack) && operatorFromStack.Op.IsFunction)
                        {
                            operatorFromStack = operatorStack.Pop();
                            outputNodes.Push(ApplyOperator(nodeFactory, outputNodes, operatorFromStack, expression));
                        }
                        break;
                    default:
                        throw new UncatchableParserException("Unsupported token type received from lexer: " + token.Type.ToString());
                }

                trackingState = (token.Type == TokenType.Number || token.Type == TokenType.ClosingBracket) ? TrackingState.OperatorExpected : TrackingState.OperandExpected;
            }

            while (operatorStack.TryPop(out var operatorFromStack))
            {
                outputNodes.Push(ApplyOperator(nodeFactory, outputNodes, operatorFromStack, expression));
            }

            if (outputNodes.Count == 0)
                throw new InvalidExpressionException("Empty expression", 0);

            if (outputNodes.Count > 1)
                throw new UnbalancedExpressionException("Unbalanced expression passed", expression.Length);

            return outputNodes.Pop();
        }



        private static ValueTask<TNode> ApplyOperatorAsync<TNodeFactory, TNode>(TNodeFactory nodeFactory, Stack<TNode> args, ExpressionOperationExt oper, string expression)
            where TNodeFactory : IAsyncExpressionNodesFactory<TNode>
        {
            Debug.Assert(oper.Op.OperationType != null);

            switch (oper.Op.OperandCount)
            {
                case 1:
                    if (args.Count < 1)
                        return ValueTask.FromException<TNode>(new UnbalancedExpressionException($"Operation {oper.Op.OperationType} expected 1 operand which is not provided. Offset = {oper.Offset}. Context = {GetTextAroundOffset(expression, oper.Offset)}", 0));
                    var lastArg = args.Pop();
                    return nodeFactory.UnaryOpAsync(oper.Op.OperationType.Value, oper.Offset, lastArg);
                case 2:
                    if (args.Count < 2)
                        return ValueTask.FromException<TNode>(new UnbalancedExpressionException($"Operation {oper.Op.OperationType} expected 2 operands which is not provided. Offset = {oper.Offset}. Context = {GetTextAroundOffset(expression, oper.Offset)}", 0));
                    var arg2 = args.Pop();
                    var arg1 = args.Pop();
                    return nodeFactory.BinaryOpAsync(oper.Op.OperationType.Value, oper.Offset, arg1, arg2);
                default:
                    return ValueTask.FromException<TNode>(new UncatchableParserException("Unsupported number of operands: " + oper.Op.OperandCount.ToString()));
            }
        }

        /// <summary>
        /// Parses math expression using modified Shunting Yard algorithm
        /// </summary>
        /// <param name="expression">String with expression</param>
        /// <param name="nodeFactory">Factory to create nodes</param>
        /// <returns></returns>
        /// <exception cref="InvalidLexemaException">Invalid lexema found</exception>
        /// <exception cref="InvalidExpressionException">General problems with passed expression</exception>
        /// <exception cref="UnbalancedExpressionException">Opening closing braces mimatch, or operator arguments mismatch</exception>
        /// <exception cref="InvalidNumberException">Found number that cannot be parsed</exception>
        /// <exception cref="UnknownIdentifierException">Found unknown function identifier</exception>
        public static async ValueTask<TNode> ParseExpressionAsync<TNodeFactory, TNode>(string expression, TNodeFactory nodeFactory)
            where TNodeFactory : IAsyncExpressionNodesFactory<TNode>
        {
            Stack<TNode> outputNodes = new Stack<TNode>();
            Stack<ExpressionOperationExt> operatorStack = new Stack<ExpressionOperationExt>();
            TrackingState trackingState = TrackingState.OperandExpected;

            foreach (var token in TokenStream.EnumerateTokens(expression, allowErrors: true))
            {
                if (token.IsError)
                    throw new InvalidLexemaException($"{token.ErrorDescription}. Offset = {token.Offset}. Value = '{token.GetTokenTextDebug()}'", token.Offset, token.Length);

                ExpressionOperationExt operatorFromStack;

                // Require brackets after function
                if (operatorStack.TryPeek(out operatorFromStack) &&
                    operatorFromStack.Op.IsFunction &&
                    token.Type != TokenType.OpeningBracket)
                {
                    throw new InvalidExpressionException($"Opening bracket expected after the function name, but found {token.Type}. Offset = {token.Offset}. Context = '{GetTextAroundOffset(expression, token.Offset)}'", token.Offset);
                }

                switch (token.Type)
                {
                    case TokenType.Number:
                        if (trackingState != TrackingState.OperandExpected)
                            throw new InvalidExpressionException($"Operator expected, but number is found. Offset = {token.Offset}. Context = '{GetTextAroundOffset(expression, token.Offset)}'", token.Offset);

                        var node = await nodeFactory.NumberAsync(token.GetTokenText(), token.Offset);
                        outputNodes.Push(node);
                        break;
                    case TokenType.Identifier:
                        if (trackingState != TrackingState.OperandExpected)
                            throw new InvalidExpressionException($"Operator expected, but function call is found. Offset = {token.Offset}. Context = '{GetTextAroundOffset(expression, token.Offset)}'", token.Offset);

                        var func = GetFunctionNameForIdentifier(token);
                        operatorStack.Push(func);
                        break;
                    case TokenType.Plus when trackingState == TrackingState.OperandExpected:
                    case TokenType.Minus when trackingState == TrackingState.OperandExpected:
                        var newUnaryOperator = ExpressionOperation.GetUnaryOperatorForLexerToken(token.Type);
                        operatorStack.Push(new ExpressionOperationExt(newUnaryOperator, token.Offset));
                        break;
                    case TokenType.Plus:
                    case TokenType.Minus:
                    case TokenType.MultiplicationSign:
                    case TokenType.DivisionSign:
                    case TokenType.ExponentSign:
                        if (trackingState != TrackingState.OperatorExpected)
                            throw new InvalidExpressionException($"Unexpected operator sequence. Offset = {token.Offset}. Context = '{GetTextAroundOffset(expression, token.Offset)}'", token.Offset);

                        var newOperator = ExpressionOperation.GetOperatorForLexerToken(token.Type);
                        while (operatorStack.TryPeek(out operatorFromStack) &&
                            operatorFromStack.Op != ExpressionOperation.OpeningBracket &&
                            (operatorFromStack.Op.Priority > newOperator.Priority || (operatorFromStack.Op.Priority == newOperator.Priority && newOperator.Associativity == OperatorAssociativity.Left)))
                        {
                            operatorFromStack = operatorStack.Pop();
                            outputNodes.Push(await ApplyOperatorAsync(nodeFactory, outputNodes, operatorFromStack, expression));
                        }
                        operatorStack.Push(new ExpressionOperationExt(newOperator, token.Offset));
                        break;
                    case TokenType.OpeningBracket:
                        if (trackingState != TrackingState.OperandExpected)
                            throw new InvalidExpressionException($"Unexpected opening bracket. Offset = {token.Offset}. Context = '{GetTextAroundOffset(expression, token.Offset)}'", token.Offset);

                        operatorStack.Push(new ExpressionOperationExt(ExpressionOperation.OpeningBracket, token.Offset));
                        break;
                    case TokenType.ClosingBracket:
                        if (trackingState != TrackingState.OperatorExpected)
                            throw new InvalidExpressionException($"Unexpected closing bracket. Offset = {token.Offset}. Context = '{GetTextAroundOffset(expression, token.Offset)}'", token.Offset);

                        while (operatorStack.TryPeek(out operatorFromStack) &&
                               operatorFromStack.Op != ExpressionOperation.OpeningBracket)
                        {
                            operatorFromStack = operatorStack.Pop();
                            outputNodes.Push(await ApplyOperatorAsync(nodeFactory, outputNodes, operatorFromStack, expression));
                        }

                        if (!operatorStack.TryPeek(out operatorFromStack) || operatorFromStack.Op != ExpressionOperation.OpeningBracket)
                            throw new UnbalancedExpressionException($"Closing bracket without paired opening bracket found. Offset = {token.Offset}. Context = '{GetTextAroundOffset(expression, token.Offset)}'", token.Offset);
                        operatorStack.Pop();

                        // Check function
                        if (operatorStack.TryPeek(out operatorFromStack) && operatorFromStack.Op.IsFunction)
                        {
                            operatorFromStack = operatorStack.Pop();
                            outputNodes.Push(await ApplyOperatorAsync(nodeFactory, outputNodes, operatorFromStack, expression));
                        }
                        break;
                    default:
                        throw new UncatchableParserException("Unsupported token type received from lexer: " + token.Type.ToString());
                }

                trackingState = (token.Type == TokenType.Number || token.Type == TokenType.ClosingBracket) ? TrackingState.OperatorExpected : TrackingState.OperandExpected;
            }

            while (operatorStack.TryPop(out var operatorFromStack))
            {
                outputNodes.Push(await ApplyOperatorAsync(nodeFactory, outputNodes, operatorFromStack, expression));
            }

            if (outputNodes.Count == 0)
                throw new InvalidExpressionException("Empty expression", 0);

            if (outputNodes.Count > 1)
                throw new UnbalancedExpressionException("Unbalanced expression passed", expression.Length);

            return outputNodes.Pop();
        }
    }
}
