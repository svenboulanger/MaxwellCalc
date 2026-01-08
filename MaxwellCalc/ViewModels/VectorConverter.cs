using Avalonia;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MaxwellCalc.ViewModels;

/// <summary>
/// A JSON converter that can be used to serialize Avalonia's vector.
/// </summary>
public class VectorConverter : JsonConverter<Avalonia.Vector>
{
    /// <inheritdoc />
    public override Vector Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        Vector result;
        switch (reader.TokenType)
        {
            case JsonTokenType.StartArray:
                {
                    reader.Read();

                    // Read X
                    if (reader.TokenType != JsonTokenType.Number)
                        throw new JsonException("Expected a number for the X-coordinate.");
                    double x = reader.GetDouble();
                    reader.Read();

                    // Read Y
                    if (reader.TokenType != JsonTokenType.Number)
                        throw new JsonException("Expected a number for the Y-coordinate.");
                    double y = reader.GetDouble();
                    reader.Read();

                    // Check that the array ends
                    if (reader.TokenType != JsonTokenType.EndArray)
                        throw new JsonException("Expected only 2 elements");
                    result = new Vector(x, y);
                }
                break;

            default:
                throw new JsonException("Expected an array.");
        }
        return result;
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, Vector value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        writer.WriteNumberValue(value.X);
        writer.WriteNumberValue(value.Y);
        writer.WriteEndArray();
    }
}
