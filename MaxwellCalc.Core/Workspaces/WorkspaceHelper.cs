using MaxwellCalc.Core.Attributes;
using MaxwellCalc.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Xml.Linq;

namespace MaxwellCalc.Workspaces
{
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

            // Write the output units
            writer.WriteStartArray("output_units");
            foreach (var outputUnit in workspace.OutputUnits)
                JsonSerializer.Serialize(writer, outputUnit, options);
            writer.WriteEndArray();

            // Write the variables
            writer.WriteStartArray("variables");
            foreach (var variable in workspace.Variables)
                JsonSerializer.Serialize(writer, variable, options);
            writer.WriteEndArray();

            // Write the constants
            writer.WriteStartArray("constants");
            foreach (var constant in workspace.Constants)
                JsonSerializer.Serialize(writer, constant, options);
            writer.WriteEndArray();

            // Write the user functions
            writer.WriteStartArray("user_functions");
            foreach (var userFunction in workspace.UserFunctions)
                JsonSerializer.Serialize(writer, userFunction, options);
            writer.WriteEndArray();

            writer.WriteEndObject();
        }

        /// <summary>
        /// Reads a workspace from JSON. Note that built-in functions are not read.
        /// </summary>
        /// <param name="workspace">The workspace.</param>
        /// <param name="reader">The reader.</param>
        /// <param name="options">The options.</param>
        public static void ReadFromJson(this IWorkspace workspace, ref Utf8JsonReader reader, JsonSerializerOptions? options = null)
        {
            if (reader.TokenType != JsonTokenType.StartObject ||
                !reader.Read())
                throw new JsonException();

            while (reader.TokenType != JsonTokenType.EndObject)
            {
                if (reader.TokenType != JsonTokenType.PropertyName)
                    throw new JsonException();
                switch (reader.GetString())
                {
                    case "input_units":
                        reader.Read();
                        var inputUnits = JsonSerializer.Deserialize<IEnumerable<InputUnit>>(ref reader, options) ?? [];
                        foreach (var inputUnit in inputUnits)
                        {
                            if (!workspace.TryRegisterInputUnit(inputUnit))
                                throw new NotSupportedException(workspace.DiagnosticMessage);
                        }
                        reader.Read();
                        break;

                    case "output_units":
                        reader.Read();
                        var outputUnits = JsonSerializer.Deserialize<IEnumerable<OutputUnit>>(ref reader, options) ?? [];
                        foreach (var outputUnit in outputUnits)
                        {
                            if (!workspace.TryRegisterOutputUnit(outputUnit))
                                throw new NotSupportedException(workspace.DiagnosticMessage);
                        }
                        reader.Read();
                        break;

                    case "variables":
                        reader.Read();
                        var variables = JsonSerializer.Deserialize<IEnumerable<Variable>>(ref reader, options) ?? [];
                        foreach (var variable in variables)
                        {
                            if (!workspace.TrySetVariable(variable))
                                throw new NotSupportedException(workspace.DiagnosticMessage);
                        }
                        reader.Read();
                        break;

                    case "constants":
                        reader.Read();
                        var constants = JsonSerializer.Deserialize<IEnumerable<Variable>>(ref reader, options) ?? [];
                        foreach (var variable in constants)
                        {
                            if (!workspace.TrySetConstant(variable))
                                throw new NotSupportedException(workspace.DiagnosticMessage);
                        }
                        reader.Read();
                        break;

                    case "user_functions":
                        reader.Read();
                        var userFunctions = JsonSerializer.Deserialize<IEnumerable<UserFunction>>(ref reader, options) ?? [];
                        foreach (var function in userFunctions)
                        {
                            if (!workspace.TryRegisterUserFunction(function))
                                throw new NotSupportedException(workspace.DiagnosticMessage);
                        }
                        reader.Read();
                        break;

                    default:
                        throw new Exception();
                }
            }
            if (reader.TokenType != JsonTokenType.EndObject)
                throw new JsonException();
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

            // Get the method to register a new built-in function
            var registerFunctionDelegate = workspace.GetType().GetMethod("TryRegisterBuiltInFunction", BindingFlags.Public | BindingFlags.Instance);
            if (registerFunctionDelegate is null)
                throw new NotImplementedException();
            var functionDelegateType = registerFunctionDelegate.GetParameters()[1].ParameterType;

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
                registerFunctionDelegate.Invoke(workspace,
                [
                    name,
                    member.CreateDelegate(functionDelegateType),
                    new BuiltInFunction(name, minArgCount, maxArgCount, description)
                ]);
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

            // Get the property that defines the scope
            var scopeProperty = workspace.GetType().GetProperty("ConstantsScope", BindingFlags.Public | BindingFlags.Instance);
            if (scopeProperty is null || scopeProperty.PropertyType != scopeType)
                throw new NotImplementedException();
            var scope = scopeProperty.GetValue(workspace);
            var setConstantMethod = scopeProperty.PropertyType.GetMethod("TrySetVariable", [typeof(string), quantityType, typeof(string)]);
            if (setConstantMethod is null)
                throw new NotImplementedException();

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
                        workspace.TrySetConstant(new Variable(name, new Quantity<string>((string)field.GetRawConstantValue(), Unit.UnitNone), description));
                    else if (field.FieldType == domainType)
                        setConstantMethod.Invoke(scope, [name, Activator.CreateInstance(quantityType, field.GetRawConstantValue(), Unit.UnitNone), description]);
                }
                else
                {
                    // Deal with static constants or units
                    if (field.FieldType == typeof(string))
                        workspace.TrySetConstant(new Variable(name, new Quantity<string>((string)field.GetValue(null), Unit.UnitNone), description));
                    else if (field.FieldType == typeof(Quantity<string>))
                        workspace.TrySetConstant(new Variable(name, (Quantity<string>)field.GetValue(null), description));
                    else if (field.FieldType == domainType)
                        setConstantMethod.Invoke(scope, [name, Activator.CreateInstance(quantityType, field.GetValue(null), Unit.UnitNone), description]);
                    else if (field.FieldType == quantityType)
                        setConstantMethod.Invoke(scope, [name, field.GetValue(null), description]);
                }
            }
        }
    }
}
