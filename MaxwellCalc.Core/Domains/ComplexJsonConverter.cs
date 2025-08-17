using System;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MaxwellCalc.Core.Domains
{
    /// <summary>
    /// A <see cref="JsonConverter{T}"/> that works for a <see cref="ComplexDomain"/>.
    /// </summary>
    public class ComplexJsonConverter : JsonConverter<Complex>
    {
        /// <inheritdoc />
        public override Complex Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            Complex result;
            switch (reader.TokenType)
            {
                case JsonTokenType.Number:
                    result = new Complex(reader.GetDouble(), 0.0);
                    break;

                case JsonTokenType.StartArray:
                    {
                        reader.Read();

                        // Read the real part
                        if (reader.TokenType != JsonTokenType.Number)
                            throw new JsonException("Expected a number for the real part");
                        double real = reader.GetDouble();
                        reader.Read();

                        // Read the imaginary part
                        if (reader.TokenType != JsonTokenType.Number)
                            throw new JsonException("Expected a number for the imaginary part");
                        double imaginary = reader.GetDouble();
                        reader.Read();

                        // Check that the array ends
                        if (reader.TokenType != JsonTokenType.EndArray)
                            throw new JsonException("Expected only two scalars for the complex number");
                        result = new Complex(real, imaginary);
                    }
                    break;

                default:
                    throw new JsonException("Expected a number or an array of two scalars");
            }
            return result;
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, Complex value, JsonSerializerOptions options)
        {
            if (value.Imaginary.Equals(0.0))
            {
                // We can simply write a real number
                writer.WriteNumberValue(value.Real);
            }
            else
            {
                // We need to write an array of two items
                writer.WriteStartArray();
                writer.WriteNumberValue(value.Real);
                writer.WriteNumberValue(value.Imaginary);
                writer.WriteEndArray();
            }
        }
    }
}
