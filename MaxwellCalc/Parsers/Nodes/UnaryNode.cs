using System;
using System.Numerics;
using MaxwellCalc.Resolvers;
using MaxwellCalc.Units;
using MaxwellCalc.Workspaces;

namespace MaxwellCalc.Parsers.Nodes
{
    /// <summary>
    /// Creates a new <see cref="UnaryNode"/>.
    /// </summary>
    /// <param name="type">The operator type.</param>
    /// <param name="argument">The argument.</param>
    /// <param name="content">The content as input.</param>
    public class UnaryNode(UnaryOperatorTypes type, INode argument, ReadOnlyMemory<char> content) : INode
    {
        /// <inheritdoc />
        public ReadOnlyMemory<char> Content { get; } = content;

        /// <summary>
        /// Gets the operator type.
        /// </summary>
        public UnaryOperatorTypes Type { get; } = type;

        /// <summary>
        /// Gets the argument.
        /// </summary>
        public INode Argument { get; } = argument;

        /// <inheritdoc />
        public bool TryResolve<T>(IResolver<T> resolver, IWorkspace<T>? workspace, out Quantity<T> result) where T : struct, IFormattable
        {
            if (!Argument.TryResolve(resolver, workspace, out var arg))
            {
                result = resolver.Default;
                return false;
            }

            // If the argument is requested from something in different units, then let's convert it now!
            if (Type == UnaryOperatorTypes.RemoveUnits && Argument is BinaryNode bn && bn.Type == BinaryOperatorTypes.InUnit)
            {
                if (!bn.Right.TryResolve(resolver, workspace, out var unit) ||
                    !bn.Left.TryResolve(resolver, workspace, out var value))
                {
                    result = resolver.Default;
                    return false;
                }
                else if (unit.Unit != value.Unit)
                {
                    if (workspace is not null)
                        workspace.ErrorMessage = "Cannot convert units as units don't match.";
                    result = resolver.Default;
                    return false;
                }
                else
                {
                    if (!resolver.TryDivide(value, unit, workspace, out result))
                        return false;
                    result = new Quantity<T>(result.Scalar, Unit.UnitNone);
                    return true;
                }
            }

            return Type switch
            {
                UnaryOperatorTypes.Plus => resolver.TryPlus(arg, workspace, out result),
                UnaryOperatorTypes.Minus => resolver.TryMinus(arg, workspace, out result),
                UnaryOperatorTypes.Factorial => resolver.TryFactorial(arg, workspace, out result),
                UnaryOperatorTypes.RemoveUnits => resolver.TryRemoveUnits(arg, workspace, out result),
                _ => throw new NotImplementedException(),
            };
        }
    }
}
