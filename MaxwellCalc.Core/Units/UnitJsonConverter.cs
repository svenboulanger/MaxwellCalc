using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MaxwellCalc.Core.Units;

/// <summary>
/// A JSON converter for units.
/// </summary>
public class UnitJsonConverter : JsonConverter<Unit>
{
    /// <inheritdoc />
    public override Unit Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.StartArray:
                {
                    reader.Read();
                    var dict = new Dictionary<string, Fraction>();
                    while (reader.TokenType != JsonTokenType.EndArray)
                    {
                        switch (reader.TokenType)
                        {
                            case JsonTokenType.String:
                                {
                                    // Single unit case
                                    string unit = reader.GetString() ?? throw new JsonException("Expected a unit name");
                                    reader.Read();

                                    // Optional numerator
                                    Fraction fraction = 1;
                                    if (reader.TokenType == JsonTokenType.Number)
                                    {
                                        int numerator = reader.GetInt32();
                                        int denominator = 1;
                                        reader.Read();

                                        // Optional denominator
                                        if (reader.TokenType == JsonTokenType.Number)
                                        {
                                            denominator = reader.GetInt32();
                                            reader.Read();
                                        }
                                        dict[unit] = new(numerator, denominator);
                                    }
                                    else
                                        dict[unit] = 1;

                                    if (reader.TokenType != JsonTokenType.EndArray)
                                        throw new JsonException("Expected up to 3 elements for a unit dimension");
                                }
                                break;

                            case JsonTokenType.StartArray:
                                {
                                    reader.Read();

                                    // Read the unit
                                    string unit = reader.GetString() ?? throw new JsonException("Expected a unit name");
                                    reader.Read();

                                    // Optional numerator
                                    Fraction fraction = 1;
                                    if (reader.TokenType == JsonTokenType.Number)
                                    {
                                        int numerator = reader.GetInt32();
                                        int denominator = 1;
                                        reader.Read();

                                        // Optional denominator
                                        if (reader.TokenType == JsonTokenType.Number)
                                        {
                                            denominator = reader.GetInt32();
                                            reader.Read();
                                        }
                                        dict[unit] = new(numerator, denominator);
                                    }
                                    else
                                        dict[unit] = 1;

                                    if (reader.TokenType != JsonTokenType.EndArray)
                                        throw new JsonException("Expected up to 3 elements for a unit dimension");
                                    reader.Read();
                                }
                                break;

                            default:
                                throw new JsonException("Unrecognized token for a unit");
                        }
                    }

                    return new([.. dict.Select(p => (p.Key, p.Value))]);
                }

            case JsonTokenType.String:
                {
                    // Simple unit
                    string unit = reader.GetString() ?? throw new JsonException("Expected a unit");
                    return new Unit((unit, 1));
                }

            default:
                throw new JsonException("");
        }
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, Unit value, JsonSerializerOptions options)
    {
        // No units
        if (value.Dimension is null)
        {
            writer.WriteStartArray();
            writer.WriteEndArray();
            return;
        }

        // Shorthand notation for a simple unit
        if (value.Dimension.Count == 1)
        {
            var pair = value.Dimension.First();
            if (pair.Value.Numerator == 1 && pair.Value.Denominator == 1)
            {
                // Simple unit name
                writer.WriteStringValue(pair.Key);
                return;
            }

            // Simple array instead of nested
            writer.WriteStartArray();
            writer.WriteStringValue(pair.Key);
            writer.WriteNumberValue(pair.Value.Numerator);
            if (pair.Value.Denominator != 1)
                writer.WriteNumberValue(pair.Value.Denominator);
            writer.WriteEndArray();
            return;
        }

        // Combinations of multiple units
        writer.WriteStartArray();
        foreach (var pair in value.Dimension)
        {
            // Always write an array to distinguish with the 1-dimensional case
            writer.WriteStartArray();
            writer.WriteStringValue(pair.Key);
            if (pair.Value.Numerator != 1 || pair.Value.Denominator != 1)
            {
                writer.WriteNumberValue(pair.Value.Numerator);
                if (pair.Value.Denominator != 1)
                    writer.WriteNumberValue(pair.Value.Denominator);
            }
            writer.WriteEndArray();
        }
        writer.WriteEndArray();
    }
}
