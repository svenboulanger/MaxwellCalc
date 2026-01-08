using MaxwellCalc.Core.Units;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MaxwellCalc.Core.Workspaces.Variables;

/// <summary>
/// A <see cref="JsonConverter{T}"/> for a <see cref="Variable{T}"/>.
/// </summary>
/// <typeparam name="T">The scalar type.</typeparam>
public class VariableJsonConverter<T> : JsonConverter<Variable<T>>
{
    /// <inheritdoc />
    public override Variable<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected an object for a variable");
        reader.Read();

        Quantity<T> quantity = default;
        string? description = null;
        while (reader.TokenType != JsonTokenType.EndObject)
        {
            // Gets the property name
            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException("Expected a property name for the variable");
            string propertyName = reader.GetString() ?? throw new JsonException("Expected a property name for the variable");
            reader.Read();

            switch (propertyName)
            {
                case "q":
                    quantity = JsonSerializer.Deserialize<Quantity<T>>(ref reader, options);
                    break;

                case "d":
                    description = reader.GetString();
                    break;

                default:
                    throw new JsonException($"Unrecognized property name '{propertyName}'");
            }
            reader.Read();
        }
        return new Variable<T>(quantity, description);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, Variable<T> value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        
        // The quantity
        writer.WritePropertyName("q");
        JsonSerializer.Serialize(writer, value.Value, options);

        // The description
        if (value.Description != null)
            writer.WriteString("d", value.Description);

        writer.WriteEndObject();
    }
}
