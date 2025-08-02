using MaxwellCalc.Parsers.Nodes;
using MaxwellCalc.Workspaces;
using System;
using System.Collections.Generic;

namespace MaxwellCalc.Parsers
{
    /// <summary>
    /// The parser.
    /// </summary>
    public static class Parser
    {
        /// <summary>
        /// Parses an expression.
        /// </summary>
        /// <param name="lexer">The lexer.</param>
        /// <param name="workspace">The workspace.</param>
        /// <returns>Returns the node.</returns>
        public static INode Parse(Lexer lexer, IWorkspace workspace)
            => ParseAssignment(lexer, workspace);

        private static INode ParseAssignment(Lexer lexer, IWorkspace workspace)
        {
            int start = lexer.Column;
            var result = ParseUnitConversion(lexer, workspace);
            if (lexer.Type == TokenTypes.Assignment)
            {
                // Right associative
                lexer.Next();
                var b = ParseAssignment(lexer, workspace);
                result = new BinaryNode(BinaryOperatorTypes.Assign, result, b, lexer.Track(start));
            }
            return result;
        }

        private static INode ParseUnitConversion(Lexer lexer, IWorkspace workspace)
        {
            var start = lexer.Column;
            var result = ParseBitwiseOr(lexer, workspace);
            while (lexer.Type == TokenTypes.Word && lexer.Content.ToString() == "in")
            {
                lexer.Next();
                var toQuantity = ParseBitwiseOr(lexer, workspace);
                result = new BinaryNode(BinaryOperatorTypes.InUnit, result, toQuantity, lexer.Track(start));
            }
            return result;
        }

        private static INode ParseBitwiseOr(Lexer lexer, IWorkspace workspace)
        {
            int start = lexer.Column;
            var result = ParseBitwiseAnd(lexer, workspace);
            while (lexer.Type == TokenTypes.BitwiseOr)
            {
                lexer.Next();
                var b = ParseBitwiseAnd(lexer, workspace);
                result = new BinaryNode(BinaryOperatorTypes.BitwiseOr, result, b, lexer.Track(start));
            }
            return result;
        }

        private static INode ParseBitwiseAnd(Lexer lexer, IWorkspace workspace)
        {
            int start = lexer.Column;
            var result = ParseArithmeticShift(lexer, workspace);
            while (lexer.Type == TokenTypes.BitwiseAnd)
            {
                lexer.Next();
                var b = ParseArithmeticShift(lexer, workspace);
                result = new BinaryNode(BinaryOperatorTypes.BitwiseAnd, result, b, lexer.Track(start));
            }
            return result;
        }

        private static INode ParseArithmeticShift(Lexer lexer, IWorkspace workspace)
        {
            int start = lexer.Column;
            var result = ParseAddition(lexer, workspace);
            while (lexer.Type == TokenTypes.ArithmeticShift)
            {
                bool right = lexer.Content.ToString() == ">>";
                lexer.Next();
                var b = ParseAddition(lexer, workspace);
                result = new BinaryNode(right ? BinaryOperatorTypes.RightShift : BinaryOperatorTypes.LeftShift, result, b, lexer.Track(start));
            }
            return result;
        }

        private static INode ParseAddition(Lexer lexer, IWorkspace workspace)
        {
            int start = lexer.Column;
            var result = ParseMultiplication(lexer, workspace);
            while (lexer.Type == TokenTypes.Plus || lexer.Type == TokenTypes.Minus)
            {
                bool add = lexer.Type == TokenTypes.Plus;
                lexer.Next();
                var b = ParseMultiplication(lexer, workspace);
                result = new BinaryNode(add ? BinaryOperatorTypes.Add : BinaryOperatorTypes.Subtract, result, b, lexer.Track(start));
            }
            return result;
        }

        private static INode ParseMultiplication(Lexer lexer, IWorkspace workspace)
        {
            int start = lexer.Column;
            var result = ParseIntegerDivision(lexer, workspace);
            while (lexer.Type == TokenTypes.Multiply ||
                lexer.Type == TokenTypes.Divide ||
                lexer.Type == TokenTypes.OpenParenthesis ||
                lexer.Type == TokenTypes.Word && lexer.Content.ToString() != "in")
            {
                INode b;
                switch (lexer.Type)
                {
                    case TokenTypes.Multiply:
                        lexer.Next();
                        b = ParseIntegerDivision(lexer, workspace);
                        result = new BinaryNode(BinaryOperatorTypes.Multiply, result, b, lexer.Track(start));
                        break;

                    case TokenTypes.Divide:
                        lexer.Next();
                        b = ParseIntegerDivision(lexer, workspace);
                        result = new BinaryNode(BinaryOperatorTypes.Divide, result, b, lexer.Track(start));
                        break;

                    case TokenTypes.Word:
                        // Implicit multiplication
                        var name = lexer.Content;
                        lexer.Next();
                        if (workspace.IsUnit(name.ToString()))
                        {
                            // Units will have exponent precedence
                            b = ParseExponentiation(lexer, workspace);
                        }
                        else
                            b = new VariableNode(name);
                        result = new BinaryNode(BinaryOperatorTypes.Multiply, result, b, lexer.Track(start));
                        break;

                    case TokenTypes.OpenParenthesis:
                        // Implicit multiplication
                        lexer.Next();
                        b = ParseAssignment(lexer, workspace);
                        if (lexer.Type != TokenTypes.CloseParenthesis)
                            throw new ArgumentException("Unclosed parenthesis");
                        lexer.Next();
                        result = new BinaryNode(BinaryOperatorTypes.Multiply, result, b, lexer.Track(start));
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }
            return result;
        }

        private static INode ParseIntegerDivision(Lexer lexer, IWorkspace workspace)
        {
            int start = lexer.Column;
            var result = ParseUnary(lexer, workspace);
            while (lexer.Type == TokenTypes.IntegerDivision)
            {
                lexer.Next();
                var b = ParseUnary(lexer, workspace);
                result = new BinaryNode(BinaryOperatorTypes.IntDivide, result, b, lexer.Track(start));
            }
            return result;
        }

        private static INode ParseUnary(Lexer lexer, IWorkspace workspace)
        {
            if (lexer.Type == TokenTypes.Plus)
            {
                int start = lexer.Column;
                lexer.Next();
                return new UnaryNode(UnaryOperatorTypes.Plus, ParseUnary(lexer, workspace), lexer.Track(start));
            }
            if (lexer.Type == TokenTypes.Minus)
            {
                int start = lexer.Column;
                lexer.Next();
                return new UnaryNode(UnaryOperatorTypes.Minus, ParseUnary(lexer, workspace), lexer.Track(start));
            }
            if (lexer.Type == TokenTypes.Quote)
            {
                int start = lexer.Column;
                lexer.Next();
                return new UnaryNode(UnaryOperatorTypes.RemoveUnits, ParseUnary(lexer, workspace), lexer.Track(start));
            }
            return ParseExponentiation(lexer, workspace);
        }

        private static INode ParseExponentiation(Lexer lexer, IWorkspace workspace)
        {
            int start = lexer.Column;
            var result = ParseFactorial(lexer, workspace);
            if (lexer.Type == TokenTypes.Power)
            {
                lexer.Next();
                var b = ParseExponentiation(lexer, workspace);
                result = new BinaryNode(BinaryOperatorTypes.Exponent, result, b, lexer.Track(start));
            }
            return result;
        }

        private static INode ParseFactorial(Lexer lexer, IWorkspace workspace)
        {
            int start = lexer.Column;
            var result = ParseElementary(lexer, workspace);
            while (lexer.Type == TokenTypes.Exclamation)
            {
                lexer.Next();
                result = new UnaryNode(UnaryOperatorTypes.Factorial, result, lexer.Track(start));
            }
            return result;
        }

        private static INode ParseElementary(Lexer lexer, IWorkspace workspace)
        {
            // Deal with parenthesis
            if (lexer.Type == TokenTypes.OpenParenthesis)
            {
                lexer.Next();
                var result = ParseAssignment(lexer, workspace);
                if (lexer.Type != TokenTypes.CloseParenthesis)
                    throw new ArgumentException("Unclosed parenthesis");
                lexer.Next();
                return result;
            }

            // Deal with constants
            if (lexer.Type == TokenTypes.Scalar)
            {
                // Parse a number
                int start = lexer.Column;
                INode result = new ScalarNode(lexer.Content);
                lexer.Next();

                // If there is a unit right after it, then we will give it precedence
                while (lexer.Type == TokenTypes.Word && workspace.IsUnit(lexer.Content.ToString()))
                {
                    var b = ParseExponentiation(lexer, workspace);
                    result = new BinaryNode(BinaryOperatorTypes.Multiply, result, b, lexer.Track(start));
                }
                return result;
            }

            // Deal with variables and units
            if (lexer.Type == TokenTypes.Word)
            {
                int start = lexer.Column;
                var name = lexer.Content;
                lexer.Next();
                if (lexer.Type == TokenTypes.OpenParenthesis)
                {
                    // This is a function!
                    lexer.Next();
                    if (lexer.Type == TokenTypes.CloseParenthesis)
                    {
                        lexer.Next();
                        return new FunctionNode(name.ToString(), null, lexer.Track(start));
                    }
                    else
                    {
                        var arguments = new List<INode>
                        {
                            ParseAssignment(lexer, workspace)
                        };
                        while (lexer.Type == TokenTypes.Separator)
                        {
                            lexer.Next();
                            arguments.Add(ParseAssignment(lexer, workspace));
                        }
                        if (lexer.Type != TokenTypes.CloseParenthesis)
                            throw new ArgumentException("Unclosed parenthesis");
                        lexer.Next();
                        return new FunctionNode(name.ToString(), arguments, lexer.Track(start));
                    }
                }
                else if (workspace.IsUnit(name.ToString()))
                    return new UnitNode(name);
                else
                    return new VariableNode(name);
            }

            throw new NotImplementedException();
        }

    }
}
