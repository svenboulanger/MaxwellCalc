using MaxwellCalc.Core.Attributes;
using MaxwellCalc.Core.Dictionaries;
using MaxwellCalc.Core.Units;
using MaxwellCalc.Core.Workspaces.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MaxwellCalc.Core.Workspaces;

/// <summary>
/// Helper methods for workspaces.
/// </summary>
public static class WorkspaceHelper
{
    /// <summary>
    /// Converts an <see cref="IObservableDictionary{TKey, TValue}"/> into a read-only version.
    /// </summary>
    /// <typeparam name="TKey">The key.</typeparam>
    /// <typeparam name="TValue">The value.</typeparam>
    /// <param name="dictionary">The dictionary.</param>
    /// <returns>Returns the read-only shadow version of the dictionary.</returns>
    public static IReadOnlyObservableDictionary<TKey, TValue> AsReadOnly<TKey, TValue>(this IObservableDictionary<TKey, TValue> dictionary)
        => new MappedObservableDictionary<TKey, TValue, TValue>(dictionary, x => x);

    /// <summary>
    /// Restricts the workspace usage.
    /// </summary>
    /// <param name="workspace">The workspace.</param>
    /// <param name="resolveInputUnits">If <c>true</c>, units are resolved to their base units.</param>
    /// <param name="resolveOutputUnits">If <c>true</c>, base units are resolved to their output units.</param>
    /// <param name="allowUnits">If <c>true</c>, units can be used.</param>
    /// <param name="allowVariables">If <c>true</c>, variables can be used.</param>
    /// <param name="allowUserFunctions">If <c>true</c>, user functions can be used.</param>
    /// <returns>Returns the old state.</returns>
    public static Tuple<bool, bool, bool, bool, bool> Restrict(
        this IWorkspace workspace, 
        bool resolveInputUnits = true, 
        bool resolveOutputUnits = true, 
        bool allowUnits = true, 
        bool allowVariables = true, 
        bool allowUserFunctions = true)
    {
        var old = Tuple.Create(
            workspace.ResolveInputUnits,
            workspace.ResolveOutputUnits,
            workspace.AllowUnits,
            workspace.AllowVariables,
            workspace.AllowUserFunctions);
        workspace.ResolveInputUnits = resolveInputUnits;
        workspace.ResolveOutputUnits = resolveOutputUnits;
        workspace.AllowUnits = allowUnits;
        workspace.AllowVariables = allowVariables;
        workspace.AllowUserFunctions = allowUserFunctions;
        return old;
    }

    /// <summary>
    /// Restores a workspace state after having used <see cref="Restrict(IWorkspace, bool, bool, bool, bool, bool)"/>.
    /// </summary>
    /// <param name="workspace">The workspace.</param>
    /// <param name="oldState">The old state.</param>
    public static void Restore(this IWorkspace workspace, Tuple<bool, bool, bool, bool, bool> oldState)
    {
        workspace.ResolveInputUnits = oldState.Item1;
        workspace.ResolveOutputUnits = oldState.Item2;
        workspace.AllowUnits = oldState.Item3;
        workspace.AllowVariables = oldState.Item4;
        workspace.AllowUserFunctions = oldState.Item5;
    }

    /// <summary>
    /// Uses reflection to register all static methods on a type that match the signature of the workspace domain.
    /// </summary>
    /// <param name="workspace">The workspace.</param>
    /// <param name="type">The type.</param>
    public static void RegisterBuiltInMethods(this IWorkspace workspace, Type type)
    {
        // First we need to figure out what the domain type is of the workspace
        var workspaceType = workspace.GetType().GetInterfaces().FirstOrDefault(type =>
        {
            if (!type.IsGenericType)
                return false;
            return type.GetGenericTypeDefinition() == typeof(IWorkspace<>);
        }) ?? throw new ArgumentException("Workspace does not have a base type.");

        // Now use that base type ...
        var domainType = workspaceType.GenericTypeArguments[0];
        var quantityType = typeof(Quantity<>).MakeGenericType(domainType);
        var argType = typeof(IReadOnlyList<>).MakeGenericType(quantityType);
        var delegateType = workspaceType.GetNestedType("BuiltInFunctionDelegate", BindingFlags.Public | BindingFlags.Static)?.MakeGenericType(domainType);

        // ... to go through the static members of the parameter type
        foreach (var member in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
        {
            // Check the method signature
            if (member.ReturnType != typeof(bool))
                continue;
            var parameters = member.GetParameters();
            if (parameters.Length != 3)
                continue;
            if (parameters[0].ParameterType != argType)
                continue;
            if (parameters[1].ParameterType != typeof(IWorkspace))
                continue;
            if (!parameters[2].IsOut || parameters[2].ParameterType.GetElementType() != quantityType)
                continue;

            // We have a match! Let's register it as a built-in method
            string name = member.Name.ToLower(), description = string.Empty;
            int minArgCount = 1;
            int maxArgCount = 1;
            foreach (var attribute in member.GetCustomAttributes())
            {
                switch (attribute)
                {
                    case CalculatorNameAttribute fna: name = fna.Name; break;
                    case MinArgAttribute minA: minArgCount = minA.Minimum; break;
                    case MaxArgAttribute maxA: maxArgCount = maxA.Maximum; break;
                    case CalculatorDescriptionAttribute desc: description = desc.Description; break; 
                }
            }

            // Convert the member into a function delegate
            workspace.BuiltInFunctions[name] = new BuiltInFunction(
                name,
                minArgCount,
                maxArgCount,
                description,
                Delegate.CreateDelegate(delegateType, member));
        }
    }

    /// <summary>
    /// Uses reflection register all static properties and constants on a type that match the signature of the workspace domain.
    /// </summary>
    /// <param name="workspace">The workspace.</param>
    /// <param name="type">The type.</param>
    public static void RegisterConstants(this IWorkspace workspace, Type type)
    {
        // First we need to figure out what the domain type is of the workspace
        var workspaceType = workspace.GetType().GetInterfaces().FirstOrDefault(type =>
        {
            if (!type.IsGenericType)
                return false;
            return type.GetGenericTypeDefinition() == typeof(IWorkspace<>);
        }) ?? throw new ArgumentException("Workspace does not have a base type.");

        // Get the method to register a new built-in function
        var domainType = workspaceType.GenericTypeArguments[0];
        var quantityType = typeof(Quantity<>).MakeGenericType(domainType);
        var variableType = typeof(Variable<>).MakeGenericType(domainType);
        var scopeType = typeof(IVariableScope<>).MakeGenericType(domainType);
        var constantsScope = typeof(IWorkspace).GetProperty("Constants", BindingFlags.Public | BindingFlags.Instance).GetValue(workspace);
        var constantsDictionary = scopeType.GetProperty("Local", BindingFlags.Instance | BindingFlags.Public).GetValue(constantsScope);
        var setterProperty = typeof(IDictionary<,>).MakeGenericType(typeof(string), variableType).GetProperty("Item");

        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            // Try to find metadata
            string name = field.Name.ToLower().ToString();
            string? description = null;
            foreach (var attribute in field.GetCustomAttributes())
            {
                switch (attribute)
                {
                    case CalculatorNameAttribute fna: name = fna.Name; break;
                    case CalculatorDescriptionAttribute desc: description = desc.Description; break;
                }
            }

            if (field.IsLiteral)
            {
                if (field.FieldType == domainType)
                {
                    var quantity = Activator.CreateInstance(quantityType, field.GetValue(null), Unit.UnitNone);
                    var variable = Activator.CreateInstance(variableType, quantity, description);
                    setterProperty.SetValue(constantsDictionary, variable, [name]);
                }
            }
            else
            {
                // Deal with static constants or units
                if (field.FieldType == domainType)
                {
                    var quantity = Activator.CreateInstance(quantityType, field.GetValue(null), Unit.UnitNone);
                    var variable = Activator.CreateInstance(variableType, quantity, description);
                    setterProperty.SetValue(constantsDictionary, variable, [name]);
                }
                else if (field.FieldType == quantityType)
                {
                    var variable = Activator.CreateInstance(variableType, field.GetValue(null), description);
                    setterProperty.SetValue(constantsDictionary, variable, [name]);
                }
            }
        }
    }
}
