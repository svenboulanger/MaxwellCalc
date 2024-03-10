using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MaxwellCalc.Units
{
    /// <summary>
    /// A JSON converter for units.
    /// </summary>
    public class UnitJsonConverter : JsonConverter<Unit>
    {
        /// <inheritdoc />
        public override Unit Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            int depth = reader.CurrentDepth;
            if (reader.TokenType != JsonTokenType.StartArray ||
                !reader.Read())
            {
                reader.Skip();
                return default;
            }

            var dict = new Dictionary<string, Fraction>();
            while (reader.TokenType != JsonTokenType.EndArray)
            {
                if (reader.TokenType != JsonTokenType.StartArray ||
                    !reader.Read())
                {
                    reader.Skip();
                    return default;
                }

                // Read the dimension
                if (reader.TokenType != JsonTokenType.String)
                {
                    reader.Skip();
                    return default;
                }
                var dimension = reader.GetString();
                if (!reader.Read())
                    return default;

                // Numerator
                int numerator = 1;
                if (reader.TokenType == JsonTokenType.Number)
                {
                    numerator = reader.GetInt32();
                    if (!reader.Read())
                        return default;
                }

                // Denominator
                int denominator = 1;
                if (reader.TokenType == JsonTokenType.Number)
                {
                    denominator = reader.GetInt32();
                    if (!reader.Read())
                        return default;
                }

                if (dimension is not null)
                    dict[dimension] = new(numerator, denominator);

                // Skip the rest of the array
                while (reader.TokenType != JsonTokenType.EndArray)
                {
                    if (!reader.Read())
                        return default;
                }
                reader.Read();
            }

            // Finish
            while (reader.TokenType != JsonTokenType.EndArray && reader.CurrentDepth > depth)
                reader.Read();
            return new(dict.Select(p => (p.Key, p.Value)).ToArray());
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, Unit value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            foreach (var pair in value.Dimension)
            {
                writer.WriteStartArray();
                writer.WriteStringValue(pair.Key);
                writer.WriteNumberValue(pair.Value.Numerator);
                writer.WriteNumberValue(pair.Value.Denominator);
                writer.WriteEndArray();
            }
            writer.WriteEndArray();
        }
    }
}
