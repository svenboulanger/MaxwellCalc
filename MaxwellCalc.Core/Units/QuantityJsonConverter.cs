using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MaxwellCalc.Core.Units
{
    /// <summary>
    /// A <see cref="JsonConverter{T}"/> for a <see cref="Quantity{T}"/>. 
    /// </summary>
    /// <typeparam name="T">The type class.</typeparam>
    public class QuantityJsonConverter<T> : JsonConverter<Quantity<T>>
    {
        /// <inheritdoc />
        public override Quantity<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.StartObject:
                    {
                        if (reader.TokenType != JsonTokenType.StartObject)
                            throw new JsonException("Expected an object for a quantity");
                        reader.Read();

                        T? scalar = default;
                        Unit unit = Unit.UnitNone;
                        while (reader.TokenType != JsonTokenType.EndObject)
                        {
                            // Property name
                            if (reader.TokenType != JsonTokenType.PropertyName)
                                throw new JsonException("Expected a property name");
                            string propertyName = reader.GetString() ?? string.Empty;

                            // Value
                            switch (propertyName)
                            {
                                case "s": // Scalar
                                    scalar = JsonSerializer.Deserialize<T>(ref reader, options);
                                    break;

                                case "u": // Units
                                    unit = JsonSerializer.Deserialize<Unit>(ref reader, options);
                                    break;

                                default:
                                    throw new JsonException($"Unrecognized property '{propertyName}' for quantity");
                            }
                            reader.Read();
                        }

                        if (reader.TokenType != JsonTokenType.EndObject)
                            throw new JsonException("Expected end of object");
                        if (scalar is null)
                            throw new JsonException("Scalar cannot be null");
                        return new Quantity<T>(scalar, unit);
                    }

                default:
                    throw new JsonException("Unrecognized token for a quantity");
            }
            
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, Quantity<T> value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            // Scalar
            writer.WritePropertyName("s");
            JsonSerializer.Serialize(writer, value.Scalar, options);

            // Unit - Only export if it has no units
            if (value.Unit != Unit.UnitNone)
            {
                writer.WritePropertyName("u");
                JsonSerializer.Serialize(writer, value.Unit, options);
            }

            writer.WriteEndObject();
        }
    }
}
