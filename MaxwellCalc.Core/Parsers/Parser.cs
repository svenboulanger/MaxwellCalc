using MaxwellCalc.Core.Parsers.Nodes;
using MaxwellCalc.Parsers.Nodes;
using MaxwellCalc.Workspaces;
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
        public static INode? Parse(Lexer lexer, IWorkspace workspace)
        {
            if (lexer.Type == TokenTypes.EndOfLine)
                return null;
            var node = ParseAssignment(lexer, workspace);
            if (lexer.Type != TokenTypes.EndOfLine)
            {
                workspace.DiagnosticMessage = $"Unrecognized token at column {lexer.Index + 1}.";
                return null;
            }
            return node;
        }

        private static INode? ParseAssignment(Lexer lexer, IWorkspace workspace)
        {
            int start = lexer.Index;

            // Left hand side
            var result = ParseTernary(lexer, workspace);
            if (result is null)
                return null;

            if (lexer.Type == TokenTypes.Assignment)
            {
                // Right associative
                lexer.Next();
                var b = ParseAssignment(lexer, workspace);
                if (b is null)
                    return null;

                result = new BinaryNode(BinaryOperatorTypes.Assign, result, b, lexer.Track(start));
            }
            return result;
        }

        private static INode? ParseTernary(Lexer lexer, IWorkspace workspace)
        {
            int start = lexer.Index;

            // Parse the condition
            var condition = ParseConditionalOr(lexer, workspace);
            if (condition is null)
                return null;

            if (lexer.Type == TokenTypes.Question)
            {
                lexer.Next();

                // True branch
                var ifTrue = ParseTernary(lexer, workspace);
                if (ifTrue is null)
                    return null;

                if (lexer.Type != TokenTypes.Colon)
                {
                    workspace.DiagnosticMessage = $"Expected a colon at column {lexer.Index + 1}.";
                    return null;
                }
                lexer.Next();

                // False branch
                var ifFalse = ParseTernary(lexer, workspace);
                if (ifFalse is null)
                    return null;

                return new TernaryNode(TernaryNodeTypes.Condition, condition, ifTrue, ifFalse, lexer.Track(start));
            }
            return condition;
        }

        private static INode? ParseConditionalOr(Lexer lexer, IWorkspace workspace)
        {
            int start = lexer.Index;

            // Left argument.
            var result = ParseConditionalAnd(lexer, workspace);
            if (result is null)
                return null;

            while (lexer.Type == TokenTypes.LogicalOr)
            {
                lexer.Next();

                // Right argument
                var b = ParseConditionalAnd(lexer, workspace);
                if (b is null)
                    return null;
                result = new BinaryNode(BinaryOperatorTypes.LogicalOr, result, b, lexer.Track(start));
            }
            return result;
        }

        private static INode? ParseConditionalAnd(Lexer lexer, IWorkspace workspace)
        {
            int start = lexer.Index;

            // Left argument.
            var result = ParseUnitConversion(lexer, workspace);
            if (result is null)
                return null;

            while (lexer.Type == TokenTypes.LogicalAnd)
            {
                lexer.Next();

                // Right argument
                var b = ParseUnitConversion(lexer, workspace);
                if (b is null)
                    return null;
                result = new BinaryNode(BinaryOperatorTypes.LogicalAnd, result, b, lexer.Track(start));
            }
            return result;
        }

        private static INode? ParseUnitConversion(Lexer lexer, IWorkspace workspace)
        {
            var start = lexer.Index;

            // Argument
            var result = ParseBitwiseOr(lexer, workspace);
            if (result is null)
                return null;

            while (lexer.Type == TokenTypes.Word && lexer.Content.ToString() == "in")
            {
                lexer.Next();

                // Unit
                var toQuantity = ParseBitwiseOr(lexer, workspace);
                if (toQuantity is null)
                    return null;

                result = new BinaryNode(BinaryOperatorTypes.InUnit, result, toQuantity, lexer.Track(start));
            }
            return result;
        }

        private static INode? ParseBitwiseOr(Lexer lexer, IWorkspace workspace)
        {
            int start = lexer.Index;

            // Left argument
            var result = ParseBitwiseAnd(lexer, workspace);
            if (result is null)
                return null;

            while (lexer.Type == TokenTypes.BitwiseOr)
            {
                lexer.Next();

                // Right argument
                var b = ParseBitwiseAnd(lexer, workspace);
                if (b is null)
                    return null;

                result = new BinaryNode(BinaryOperatorTypes.BitwiseOr, result, b, lexer.Track(start));
            }
            return result;
        }

        private static INode? ParseBitwiseAnd(Lexer lexer, IWorkspace workspace)
        {
            int start = lexer.Index;

            // Left argument
            var result = ParseEquality(lexer, workspace);
            if (result is null)
                return null;

            while (lexer.Type == TokenTypes.BitwiseAnd)
            {
                lexer.Next();
            
                // Right argument
                var b = ParseEquality(lexer, workspace);
                if (b is null)
                    return null;

                result = new BinaryNode(BinaryOperatorTypes.BitwiseAnd, result, b, lexer.Track(start));
            }
            return result;
        }

        private static INode? ParseEquality(Lexer lexer, IWorkspace workspace)
        {
            int start = lexer.Index;

            // Left argument.
            var result = ParseRelational(lexer, workspace);
            if (result is null)
                return null;

            while (lexer.Type == TokenTypes.Equal || lexer.Type == TokenTypes.NotEqual)
            {
                var type = lexer.Type == TokenTypes.Equal ? BinaryOperatorTypes.Equal : BinaryOperatorTypes.NotEqual;
                lexer.Next();

                // Right argument
                var b = ParseRelational(lexer, workspace);
                if (b is null)
                    return null;
                result = new BinaryNode(type, result, b, lexer.Track(start));
            }
            return result;
        }

        private static INode? ParseRelational(Lexer lexer, IWorkspace workspace)
        {
            int start = lexer.Index;

            // Left argument.
            var result = ParseArithmeticShift(lexer, workspace);
            if (result is null)
                return null;

            while (lexer.Type == TokenTypes.GreaterThan ||
                lexer.Type == TokenTypes.GreaterThanOrEqual ||
                lexer.Type == TokenTypes.LessThan ||
                lexer.Type == TokenTypes.LessThanOrEqual)
            {
                var type = lexer.Type switch
                {
                    TokenTypes.GreaterThan => BinaryOperatorTypes.GreaterThan,
                    TokenTypes.GreaterThanOrEqual => BinaryOperatorTypes.GreaterThanOrEqual,
                    TokenTypes.LessThan => BinaryOperatorTypes.LessThan,
                    TokenTypes.LessThanOrEqual => BinaryOperatorTypes.LessThanOrEqual,
                    _ => throw new System.Exception()
                };
                lexer.Next();

                // Right argument
                var b = ParseArithmeticShift(lexer, workspace);
                if (b is null)
                    return null;
                result = new BinaryNode(type, result, b, lexer.Track(start));
            }
            return result;
        }

        private static INode? ParseArithmeticShift(Lexer lexer, IWorkspace workspace)
        {
            int start = lexer.Index;
            
            // Left argument
            var result = ParseAddition(lexer, workspace);
            if (result is null)
                return null;

            while (lexer.Type == TokenTypes.ArithmeticShift)
            {
                bool right = lexer.Content.ToString() == ">>";
                lexer.Next();

                // Right argument
                var b = ParseAddition(lexer, workspace);
                if (b is null)
                    return null;

                result = new BinaryNode(right ? BinaryOperatorTypes.RightShift : BinaryOperatorTypes.LeftShift, result, b, lexer.Track(start));
            }
            return result;
        }

        private static INode? ParseAddition(Lexer lexer, IWorkspace workspace)
        {
            int start = lexer.Index;

            // Left argument
            var result = ParseMultiplication(lexer, workspace);
            if (result is null)
                return null;

            while (lexer.Type == TokenTypes.Plus || lexer.Type == TokenTypes.Minus)
            {
                bool add = lexer.Type == TokenTypes.Plus;
                lexer.Next();

                // Right argument
                var b = ParseMultiplication(lexer, workspace);
                if (b is null)
                    return null;

                result = new BinaryNode(add ? BinaryOperatorTypes.Add : BinaryOperatorTypes.Subtract, result, b, lexer.Track(start));
            }
            return result;
        }

        private static INode? ParseMultiplication(Lexer lexer, IWorkspace workspace)
        {
            int start = lexer.Index;

            // Left argument.
            var result = ParseIntegerDivision(lexer, workspace);
            if (result is null)
                return null;

            while (lexer.Type == TokenTypes.Multiply ||
                lexer.Type == TokenTypes.Divide ||
                lexer.Type == TokenTypes.OpenParenthesis ||
                lexer.Type == TokenTypes.Word && lexer.Content.ToString() != "in")
            {
                INode? b;
                switch (lexer.Type)
                {
                    case TokenTypes.Multiply:
                        lexer.Next();

                        // Right argument
                        b = ParseIntegerDivision(lexer, workspace);
                        if (b is null)
                            return null;

                        result = new BinaryNode(BinaryOperatorTypes.Multiply, result, b, lexer.Track(start));
                        break;

                    case TokenTypes.Divide:
                        lexer.Next();

                        // Right argument
                        b = ParseIntegerDivision(lexer, workspace);
                        if (b is null)
                            return null;

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
                            if (b is null)
                                return null;
                        }
                        else
                            b = new VariableNode(name);
                        result = new BinaryNode(BinaryOperatorTypes.Multiply, result, b, lexer.Track(start));
                        break;

                    case TokenTypes.OpenParenthesis:
                        // Implicit multiplication
                        lexer.Next();
                        
                        // Right argument
                        b = ParseAssignment(lexer, workspace);
                        if (b is null)
                            return null;

                        if (lexer.Type != TokenTypes.CloseParenthesis)
                        {
                            workspace.DiagnosticMessage = $"Bracket mismatch. Closing parenthesis expected at column {lexer.Index + 1}.";
                            return null;
                        }
                        lexer.Next();
                        result = new BinaryNode(BinaryOperatorTypes.Multiply, result, b, lexer.Track(start));
                        break;

                    default:
                        workspace.DiagnosticMessage = $"Unrecognized token at column {lexer.Index + 1}.";
                        return null;
                }
            }
            return result;
        }

        private static INode? ParseIntegerDivision(Lexer lexer, IWorkspace workspace)
        {
            int start = lexer.Index;

            // Left argument
            var result = ParseUnary(lexer, workspace);
            if (result is null)
                return null;

            while (lexer.Type == TokenTypes.IntegerDivision)
            {
                lexer.Next();
            
                // Right argument
                var b = ParseUnary(lexer, workspace);
                if (b is null)
                    return null;
                result = new BinaryNode(BinaryOperatorTypes.IntDivide, result, b, lexer.Track(start));
            }
            return result;
        }

        private static INode? ParseUnary(Lexer lexer, IWorkspace workspace)
        {
            if (lexer.Type == TokenTypes.Plus)
            {
                int start = lexer.Index;
                lexer.Next();
                var arg = ParseUnary(lexer, workspace);
                if (arg is null)
                    return null;

                return new UnaryNode(UnaryOperatorTypes.Plus, arg, lexer.Track(start));
            }
            if (lexer.Type == TokenTypes.Minus)
            {
                int start = lexer.Index;
                lexer.Next();
                var arg = ParseUnary(lexer, workspace);
                if (arg is null)
                    return null;
                return new UnaryNode(UnaryOperatorTypes.Minus, arg, lexer.Track(start));
            }
            if (lexer.Type == TokenTypes.Quote)
            {
                int start = lexer.Index;
                lexer.Next();
                var arg = ParseUnary(lexer, workspace);
                if (arg is null)
                    return null;
                return new UnaryNode(UnaryOperatorTypes.RemoveUnits, arg, lexer.Track(start));
            }
            return ParseExponentiation(lexer, workspace);
        }

        private static INode? ParseExponentiation(Lexer lexer, IWorkspace workspace)
        {
            int start = lexer.Index;

            // Left argument
            var result = ParseFactorial(lexer, workspace);
            if (result is null)
                return null;

            if (lexer.Type == TokenTypes.Power)
            {
                lexer.Next();
            
                // Right argument
                var b = ParseExponentiation(lexer, workspace);
                if (b is null)
                    return null;

                result = new BinaryNode(BinaryOperatorTypes.Exponent, result, b, lexer.Track(start));
            }
            return result;
        }

        private static INode? ParseFactorial(Lexer lexer, IWorkspace workspace)
        {
            int start = lexer.Index;

            // Left argument
            var result = ParseElementary(lexer, workspace);
            if (result is null)
                return null;

            while (lexer.Type == TokenTypes.Exclamation)
            {
                lexer.Next();
                result = new UnaryNode(UnaryOperatorTypes.Factorial, result, lexer.Track(start));
            }
            return result;
        }

        private static INode? ParseElementary(Lexer lexer, IWorkspace workspace)
        {
            // Deal with parenthesis
            if (lexer.Type == TokenTypes.OpenParenthesis)
            {
                lexer.Next();

                var result = ParseAssignment(lexer, workspace);
                if (result is null)
                    return null;

                if (lexer.Type != TokenTypes.CloseParenthesis)
                {
                    workspace.DiagnosticMessage = $"Bracket mismatch. Closing parenthesis expected at column {lexer.Index + 1}.";
                    return null;
                }
                lexer.Next();
                return result;
            }

            // Deal with constants
            if (lexer.Type == TokenTypes.Scalar)
            {
                // Parse a number
                int start = lexer.Index;
                INode result = new ScalarNode(lexer.Content);
                lexer.Next();

                // If there is a unit right after it, then we will give it precedence
                while (lexer.Type == TokenTypes.Word && workspace.IsUnit(lexer.Content.ToString()))
                {
                    var b = ParseExponentiation(lexer, workspace);
                    if (b is null)
                        return null;
                    result = new BinaryNode(BinaryOperatorTypes.Multiply, result, b, lexer.Track(start));
                }
                return result;
            }

            // Deal with variables and units
            if (lexer.Type == TokenTypes.Word)
            {
                int start = lexer.Index;
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
                        var arg = ParseAssignment(lexer, workspace);
                        if (arg is null)
                            return null;

                        var arguments = new List<INode> { arg };
                        while (lexer.Type == TokenTypes.Separator)
                        {
                            lexer.Next();
                            arg = ParseAssignment(lexer, workspace);
                            if (arg is null)
                                return null;
                            arguments.Add(arg);
                        }
                        if (lexer.Type != TokenTypes.CloseParenthesis)
                        {
                            workspace.DiagnosticMessage = $"Bracket mismatch. Closing parenthesis expected at column {lexer.Index + 1}.";
                            return null;
                        }
                        lexer.Next();
                        return new FunctionNode(name.ToString(), arguments, lexer.Track(start));
                    }
                }
                else if (workspace.IsUnit(name.ToString()))
                    return new UnitNode(name);
                else
                    return new VariableNode(name);
            }

            // No clue what to do here
            workspace.DiagnosticMessage = $"Unrecognized token at column {lexer.Index + 1}.";
            return null;
        }

    }
}
