using System;
using System.Collections.Generic;
using MaxwellCalc.Resolvers;
using MaxwellCalc.Units;
using MaxwellCalc.Workspaces;

namespace MaxwellCalc.Parsers.Nodes
{
    /// <summary>
    /// A binary node.
    /// </summary>
    /// <param name="type">The operator type.</param>
    /// <param name="left">The left argument.</param>
    /// <param name="right">The right argument.</param>
    /// <param name="content">The content as input.</param>
    public class BinaryNode(BinaryOperatorTypes type, INode left, INode right, ReadOnlyMemory<char> content) : INode
    {
        /// <inheritdoc />
        public ReadOnlyMemory<char> Content { get; } = content;

        /// <summary>
        /// Gets the operator type.
        /// </summary>
        public BinaryOperatorTypes Type { get; } = type;

        /// <summary>
        /// Gets the left argument.
        /// </summary>
        public INode Left { get; } = left;

        /// <summary>
        /// Gets the right argument.
        /// </summary>
        public INode Right { get; } = right;

        /// <inheritdoc />
        public bool TryResolve<T>(IResolver<T> resolver, IWorkspace<T>? workspace, out Quantity<T> result) where T : struct, IFormattable
        {
            // Assignment is special
            if (Type == BinaryOperatorTypes.Assign)
            {
                if (Left is VariableNode variable)
                {
                    if (!Right.TryResolve(resolver, workspace, out result))
                        return false;
                    return resolver.TryAssign(variable.Content.ToString(), result, workspace, out result);
                }
                else if (Left is FunctionNode function)
                {
                    var args = new List<string>();
                    for (int i = 0; i < function.Arguments.Count; i++)
                    {
                        if (function.Arguments[i] is not VariableNode argNode)
                        {
                            if (workspace is not null)
                                workspace.ErrorMessage = "Function argument has to be a simple variable.";
                            result = resolver.Default;
                            return false;
                        }
                        args.Add(argNode.Content.ToString());
                    }
                    if (workspace is not null && workspace.TryRegisterUserFunction(function.Name, args, Right))
                    {   
                        result = resolver.Default;
                        return true;
                    }
                    result = resolver.Default;
                    return false;
                }
                else
                {
                    if (workspace is not null)
                        workspace.ErrorMessage = "Can only assign to variables or functions.";
                    result = resolver.Default;
                    return false;
                }
            }

            // Evaluate the left argument
            if (!Left.TryResolve(resolver, workspace, out var left) ||
                !Right.TryResolve(resolver, workspace, out var right))
            {
                result = resolver.Default;
                return false;
            }

            // Return the combined arguments
            return Type switch
            {
                BinaryOperatorTypes.Add => resolver.TryAdd(left, right, workspace, out result),
                BinaryOperatorTypes.Subtract => resolver.TrySubtract(left, right, workspace, out result),
                BinaryOperatorTypes.Multiply => resolver.TryMultiply(left, right, workspace, out result),
                BinaryOperatorTypes.Divide => resolver.TryDivide(left, right, workspace, out result),
                BinaryOperatorTypes.IntDivide => resolver.TryIntDivide(left, right, workspace, out result),
                BinaryOperatorTypes.Modulo => resolver.TryModulo(left, right, workspace, out result),
                BinaryOperatorTypes.LeftShift => resolver.TryLeftShift(left, right, workspace, out result),
                BinaryOperatorTypes.RightShift => resolver.TryRightShift(left, right, workspace, out result),
                BinaryOperatorTypes.BitwiseAnd => resolver.TryBitwiseAnd(left, right, workspace, out result),
                BinaryOperatorTypes.BitwiseOr => resolver.TryBitwiseOr(left, right, workspace, out result),
                BinaryOperatorTypes.Exponent => resolver.TryExp(left, right, workspace, out result),
                BinaryOperatorTypes.GreaterThan => resolver.TryGreaterThan(left, right, workspace, out result),
                BinaryOperatorTypes.GreaterThanOrEqual => resolver.TryGreaterThanOrEqual(left, right, workspace, out result),
                BinaryOperatorTypes.LessThan=> resolver.TryGreaterThan(left, right, workspace, out result),
                BinaryOperatorTypes.LessThanOrEqual => resolver.TryGreaterThanOrEqual(left, right, workspace, out result),
                BinaryOperatorTypes.Equal => resolver.TryEquals(left, right, workspace, out result),
                BinaryOperatorTypes.NotEqual => resolver.TryNotEquals(left, right, workspace, out result),
                BinaryOperatorTypes.InUnit => resolver.TryInUnit(left, right, Right.Content, workspace, out result),
                _ => throw new NotImplementedException()
            };
        }
    }
}
