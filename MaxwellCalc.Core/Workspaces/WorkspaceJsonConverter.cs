using MaxwellCalc.Core.Units;
using MaxwellCalc.Core.Workspaces.Variables;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MaxwellCalc.Core.Workspaces
{
    /// <summary>
    /// Creates a new <see cref="WorkspaceJsonConverter{T}"/>.
    /// </summary>
    /// <param name="factory">The factory method for new workspaces.</param>
    public class WorkspaceJsonConverter<T>(Func<IWorkspace<T>> factory) : JsonConverter<IWorkspace<T>>
        where T : struct, IFormattable
    {
        private readonly Func<IWorkspace<T>> _factory = factory ?? throw new ArgumentNullException(nameof(factory));

        /// <inheritdoc />
        public override IWorkspace<T>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Create the workspace
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException("Expected an object");
            reader.Read();
            var workspace = _factory();

            // Register the required JSON converters
            var subOptions = new JsonSerializerOptions(options);
            subOptions.Converters.Add(workspace.Resolver.Converter);
            subOptions.Converters.Add(new QuantityJsonConverter<T>());
            subOptions.Converters.Add(new VariableJsonConverter<T>());

            while (reader.TokenType != JsonTokenType.EndObject)
            {
                // Property name
                if (reader.TokenType != JsonTokenType.PropertyName)
                    throw new JsonException("Expected a property");
                string propertyName = reader.GetString() ?? string.Empty;
                reader.Read();

                // Values
                switch (propertyName)
                {
                    case "input_units":
                        ReadInputUnits(workspace, ref reader, subOptions);
                        break;

                    case "output_units":
                        ReadOutputUnits(workspace, ref reader, subOptions);
                        break;

                    case "constants":
                        ReadVariables((IVariableScope<T>)workspace.Constants, ref reader, subOptions);
                        break;

                    case "variables":
                        ReadVariables((IVariableScope<T>)workspace.Variables, ref reader, subOptions);
                        break;

                    default:
                        throw new JsonException($"Unrecognized property '{propertyName}'");
                }
                reader.Read();
            }
            return workspace;
        }

        private void ReadInputUnits(IWorkspace<T> workspace, ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException("Expected an object for input units");
            reader.Read();

            while (reader.TokenType != JsonTokenType.EndObject)
            {
                // Property name
                if (reader.TokenType != JsonTokenType.PropertyName)
                    throw new JsonException("Expected a unit name");
                string inputUnit = reader.GetString() ?? throw new JsonException("Expected a unit name");
                reader.Read();

                // Input unit value
                Quantity<T> quantity = JsonSerializer.Deserialize<Quantity<T>>(ref reader, options);
                reader.Read();

                // Add to the workspace
                workspace.InputUnits.Add(inputUnit, quantity);
            }
        }

        private void ReadOutputUnits(IWorkspace<T> workspace, ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
                throw new JsonException("Expected an array for output units");
            reader.Read();

            while (reader.TokenType != JsonTokenType.EndArray)
            {
                if (reader.TokenType != JsonTokenType.StartObject)
                    throw new JsonException("Expected an object for an output unit");
                reader.Read();

                T scalar = default;
                Unit outputUnit = Unit.UnitNone, baseUnit = Unit.UnitNone;
                while (reader.TokenType != JsonTokenType.EndObject)
                {
                    // Property name
                    if (reader.TokenType != JsonTokenType.PropertyName)
                        throw new JsonException("Expected a property name");
                    string propertyName = reader.GetString() ?? throw new JsonException("Expected a property name");
                    reader.Read();

                    switch (propertyName)
                    {
                        case "o":
                            outputUnit = JsonSerializer.Deserialize<Unit>(ref reader, options);
                            break;

                        case "b":
                            baseUnit = JsonSerializer.Deserialize<Unit>(ref reader, options);
                            break;

                        case "s":
                            scalar = JsonSerializer.Deserialize<T>(ref reader, options);
                            break;

                        default:
                            throw new JsonException($"Could not recognize property '{propertyName}' for output unit");
                    }
                    reader.Read();

                    // Write the output units
                }
                workspace.OutputUnits.Add(new(outputUnit, baseUnit), scalar);
                reader.Read();
            }
        }

        private void ReadVariables(IVariableScope<T> scope, ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException("Expected an object for variables");
            reader.Read();

            while (reader.TokenType != JsonTokenType.EndObject)
            {
                // Variable name
                if (reader.TokenType != JsonTokenType.PropertyName)
                    throw new JsonException("Expected a variable name");
                string variableName = reader.GetString() ?? throw new JsonException("Expected a variable name");
                reader.Read();

                // Variable value
                var variableValue = JsonSerializer.Deserialize<Variable<T>>(ref reader, options);
                reader.Read();

                scope.Local[variableName] = variableValue;
            }
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, IWorkspace<T> value, JsonSerializerOptions options)
        {
            // Register the required JSON converters
            var subOptions = new JsonSerializerOptions(options);
            subOptions.Converters.Add(value.Resolver.Converter);
            subOptions.Converters.Add(new QuantityJsonConverter<T>());
            subOptions.Converters.Add(new VariableJsonConverter<T>());

            writer.WriteStartObject();

            // Start with the input units
            writer.WriteStartObject("input_units");
            foreach (var pair in value.InputUnits)
            {
                writer.WritePropertyName(pair.Key);
                JsonSerializer.Serialize(writer, pair.Value, subOptions);
            }
            writer.WriteEndObject();

            // Then write the output units
            writer.WriteStartArray("output_units");
            foreach (var pair in value.OutputUnits)
            {
                writer.WriteStartObject();

                // Output units
                writer.WritePropertyName("o");
                JsonSerializer.Serialize(writer, pair.Key.OutputUnit, subOptions);

                // Base units
                writer.WritePropertyName("b");
                JsonSerializer.Serialize(writer, pair.Key.BaseUnit, subOptions);

                // Scalar units
                writer.WritePropertyName("s");
                JsonSerializer.Serialize(writer, pair.Value, subOptions);

                writer.WriteEndObject();
            }
            writer.WriteEndArray();

            // Write the constants
            writer.WriteStartObject("constants");
            var constants = (IVariableScope<T>)value.Constants;
            foreach (var pair in constants.Local)
            {
                writer.WritePropertyName(pair.Key);
                JsonSerializer.Serialize(writer, pair.Value, subOptions);
            }
            writer.WriteEndObject();

            // Write the variables
            writer.WriteStartObject("variables");
            var variables = (IVariableScope<T>)value.Variables;
            foreach (var pair in variables.Local)
            {
                writer.WritePropertyName(pair.Key);
                JsonSerializer.Serialize(writer, pair.Value, subOptions);
            }
            writer.WriteEndObject();

            writer.WriteEndObject();
        }
    }
}
