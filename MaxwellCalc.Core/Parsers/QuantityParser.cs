using MaxwellCalc.Core.Parsers.Nodes;
using System;

namespace MaxwellCalc.Core.Parsers;

public static class QuantityParser
{
    /// <summary>
    /// A parser for simple quantities.
    /// </summary>
    /// <param name="lexer">The lexer.</param>
    /// <returns>Returns the node.</returns>
    public static INode? Parse(Lexer lexer, out string? error)
        => ParseMultiplication(lexer, out error);

    private static INode? ParseMultiplication(Lexer lexer, out string? error)
    {
        int start = lexer.Index;
        var result = ParseUnary(lexer, out error);
        if (result is null)
            return null;

        while (lexer.Type == TokenTypes.Multiply ||
            lexer.Type == TokenTypes.Divide ||
            lexer.Type == TokenTypes.Word)
        {
            INode? b;
            switch (lexer.Type)
            {
                case TokenTypes.Multiply:
                    lexer.Next();
                    b = ParseUnary(lexer, out error);
                    if (b is null)
                        return null;
                    result = new BinaryNode(BinaryOperatorTypes.Multiply, result, b, lexer.Track(start));
                    break;

                case TokenTypes.Divide:
                    lexer.Next();
                    b = ParseUnary(lexer, out error);
                    if (b is null)
                        return null;
                    result = new BinaryNode(BinaryOperatorTypes.Divide, result, b, lexer.Track(start));
                    break;

                case TokenTypes.Word:
                    lexer.Next();
                    b = ParseExponentiation(lexer, out error);
                    if (b is null)
                        return null;
                    result = new BinaryNode(BinaryOperatorTypes.Multiply, result, b, lexer.Track(start));
                    break;

                default:
                    throw new NotImplementedException();
            }
        }
        error = null;
        return result;
    }

    private static INode? ParseUnary(Lexer lexer, out string? error)
    {
        if (lexer.Type == TokenTypes.Plus)
        {
            int start = lexer.Index;
            lexer.Next();

            var b = ParseUnary(lexer, out error);
            if (b is null)
                return null;
            return new UnaryNode(UnaryOperatorTypes.Plus, b, lexer.Track(start));
        }
        if (lexer.Type == TokenTypes.Minus)
        {
            int start = lexer.Index;
            lexer.Next();

            var b = ParseUnary(lexer, out error);
            if (b is null)
                return null;
            return new UnaryNode(UnaryOperatorTypes.Minus, b, lexer.Track(start));
        }
        return ParseExponentiation(lexer, out error);
    }

    private static INode? ParseExponentiation(Lexer lexer, out string? error)
    {
        int start = lexer.Index;
        var result = ParseElementary(lexer, out error);
        if (result is null)
            return null;

        if (lexer.Type == TokenTypes.Power)
        {
            lexer.Next();
            var b = ParseExponentiation(lexer, out error);
            if (b is null)
                return null;
            result = new BinaryNode(BinaryOperatorTypes.Exponent, result, b, lexer.Track(start));
        }
        return result;
    }

    private static INode? ParseElementary(Lexer lexer, out string? error)
    {
        if (lexer.Type == TokenTypes.OpenParenthesis)
        {
            lexer.Next();
            var result = ParseMultiplication(lexer, out error);
            if (result is null)
                return null;
            if (lexer.Type != TokenTypes.CloseParenthesis)
            {
                error = "Unmatched parenthesis";
                return null;
            }
            lexer.Next();
            return result;
        }

        // Scalars
        if (lexer.Type == TokenTypes.Scalar)
        {
            // Parse a number
            int start = lexer.Index;
            INode result = new ScalarNode(lexer.Content);
            lexer.Next();

            // If there are units right after, we give it precedence
            while (lexer.Type == TokenTypes.Word)
            {
                var b = ParseExponentiation(lexer, out error);
                if (b is null)
                    return null;
                result = new BinaryNode(BinaryOperatorTypes.Multiply, result, b, lexer.Track(start));
            }
            error = null;
            return result;
        }

        // Units
        if (lexer.Type == TokenTypes.Word)
        {
            var name = lexer.Content;
            lexer.Next();
            error = null;
            return new UnitNode(name);
        }

        error = $"Could not recognize '{lexer.Content}'.";
        return null;
    }
}
