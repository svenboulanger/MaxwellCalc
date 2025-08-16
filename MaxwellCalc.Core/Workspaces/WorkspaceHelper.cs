using MaxwellCalc.Core.Attributes;
using MaxwellCalc.Core.Workspaces;
using MaxwellCalc.Domains;
using MaxwellCalc.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MaxwellCalc.Workspaces;

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
    public static IReadonlyObservableDictionary<TKey, TValue> AsReadOnly<TKey, TValue>(this IObservableDictionary<TKey, TValue> dictionary)
        => new MappedObservableDictionary<TKey, TValue, TValue>(dictionary, x => x);

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
        var scopeType = typeof(IVariableScope<>).MakeGenericType(domainType);
        var domain = workspaceType.GetProperty("Resolver", BindingFlags.Public | BindingFlags.Instance)?.GetValue(workspace);
        var formatMethod = typeof(IDomain<>).MakeGenericType(domainType).GetMethod("TryFormat", BindingFlags.Public | BindingFlags.Instance);

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
                    var parameters = new object[] { Activator.CreateInstance(quantityType, field.GetValue(null), Unit.UnitNone), "g", System.Globalization.CultureInfo.CurrentCulture, null };
                    bool result = (bool)formatMethod.Invoke(workspace, parameters);
                    if (result)
                    {
                        var formattedResult = parameters[3];
                    }
                }
            }
            else
            {
                // Deal with static constants or units
                if (field.FieldType == domainType)
                {
                    var parameters = new object[] { Activator.CreateInstance(quantityType, field.GetValue(null), Unit.UnitNone), "g", System.Globalization.CultureInfo.CurrentCulture, null };
                    bool result = (bool)formatMethod.Invoke(workspace, parameters);
                    if (result)
                    {
                        var formattedResult = parameters[3];
                    }
                }
            }
        }
    }
}
