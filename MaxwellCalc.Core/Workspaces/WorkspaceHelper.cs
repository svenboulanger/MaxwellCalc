using MaxwellCalc.Core.Attributes;
using MaxwellCalc.Parsers;
using MaxwellCalc.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace MaxwellCalc.Workspaces;

/// <summary>
/// Helper methods for workspaces.
/// </summary>
public static class WorkspaceHelper
{
    /// <summary>
    /// Writes a workspace to JSON. Note that built-in functions are not written.
    /// </summary>
    /// <param name="workspace">The workspace.</param>
    /// <param name="writer">The writer.</param>
    /// <param name="options">The options.</param>
    public static void WriteToJson(this IWorkspace workspace, Utf8JsonWriter writer, JsonSerializerOptions? options = null)
    {
        writer.WriteStartObject();

        // Write the input units
        writer.WriteStartArray("input_units");
        foreach (var inputUnit in workspace.InputUnits)
            JsonSerializer.Serialize(writer, inputUnit, options);
        writer.WriteEndArray();

        // Write the constants
        writer.WriteStartObject("constants");
        foreach (var constant in workspace.Constants.Variables)
        {
            writer.WriteStartObject(constant.Key);
            JsonSerializer.Serialize(writer, constant.Value, options);
            writer.WriteEndObject();
        }
        writer.WriteEndArray();

        // Write the variables
        writer.WriteStartObject("variables");
        foreach (var variable in workspace.Variables.Variables)
        {
            writer.WriteStartObject(variable.Key);
            JsonSerializer.Serialize(writer, variable.Value, options);
            writer.WriteEndObject();
        }
        writer.WriteEndArray();

        // Write the output units
        writer.WriteStartArray("output_units");
        foreach (var outputUnit in workspace.OutputUnits)
            JsonSerializer.Serialize(writer, outputUnit, options);
        writer.WriteEndArray();

        // Write the user functions
        writer.WriteStartArray("user_functions");
        foreach (var userFunction in workspace.UserFunctions)
            JsonSerializer.Serialize(writer, userFunction, options);
        writer.WriteEndArray();

        writer.WriteEndObject();
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
        var scopeType = typeof(IVariableScope<>).MakeGenericType(domainType);

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
                if (field.FieldType == typeof(string))
                {
                    string expression = (string)field.GetRawConstantValue();
                    var lexer = new Lexer(expression);
                    var node = Parser.Parse(lexer, workspace);
                    if (node is not null)
                    {
                        workspace.Constants.Variables[name] = node;
                        workspace.Constants.TrySetDescription(name, description);
                    }
                }
            }
            else
            {
                // Deal with static constants or units
                if (field.FieldType == typeof(string))
                {
                    string expression = (string)field.GetValue(null);
                    var lexer = new Lexer(expression);
                    var node = Parser.Parse(lexer, workspace);
                    if (node is not null)
                    {
                        workspace.Constants.Variables[name] = node;
                        workspace.Constants.TrySetDescription(name, description);
                    }
                }
                else if (field.FieldType == typeof(Quantity<string>))
                {
                    var expression = (Quantity<string>)field.GetValue(null);
                    var lexer = new Lexer(expression.Scalar);
                    var node = Parser.Parse(lexer, workspace);
                    if (node is not null)
                    {
                        workspace.Constants.Variables[name] = node;
                        workspace.Constants.TrySetDescription(name, description);
                    }
                }
            }
        }
    }
}
