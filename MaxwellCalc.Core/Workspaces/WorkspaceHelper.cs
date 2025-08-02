using System;
using System.Collections.Generic;
using System.Text.Json;

namespace MaxwellCalc.Workspaces
{
    /// <summary>
    /// Helper methods for workspaces.
    /// </summary>
    public static class WorkspaceHelper
    {
        /// <inheritdoc />
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

            // Write the user functions
            writer.WriteStartArray("user_functions");
            foreach (var userFunction in workspace.UserFunctions)
                JsonSerializer.Serialize(writer, userFunction, options);
            writer.WriteEndArray();

            writer.WriteEndObject();
        }

        /// <inheritdoc />
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
    }
}
