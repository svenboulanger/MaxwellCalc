using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MaxwellCalc.Units
{
    public class QuantityJsonConverter : JsonConverter<IQuantity>
    {
        public override IQuantity? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                return null;
            reader.Read();

            object? scalar = null;
            List<(string, int, int)>? units = null;
            while (true)
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.PropertyName:
                        switch (reader.GetString())
                        {
                            case "s":
                                reader.Read();
                                scalar = ReadScalar(ref reader);
                                break;
                            case "u":
                                reader.Read();
                                units = ReadUnits(ref reader);
                                break;
                            default:
                                return null;
                        }
                        break;

                    case JsonTokenType.EndObject:
                        reader.Read();
                        if (scalar is double dbl)
                            return new Quantity<double>(dbl, new Unit(units?.Select(p => (p.Item1, new Fraction(p.Item2, p.Item3))).ToArray() ?? Array.Empty<(string, Fraction)>()));
                        if (scalar is Complex cplx)
                            return new Quantity<Complex>(cplx, new Unit(units?.Select(p => (p.Item1, new Fraction(p.Item2, p.Item3))).ToArray() ?? Array.Empty<(string, Fraction)>()));
                        return null;

                    default:
                        return null;
                }
            }
        }

        private object? ReadScalar(ref Utf8JsonReader reader)
        {
            object? r = null;

            // Start of array
            if (reader.TokenType != JsonTokenType.StartArray)
                return null;
            reader.Read();

            // The type of scalar
            if (reader.TokenType != JsonTokenType.String)
                return null;
            string? t = reader.GetString();
            reader.Read();

            // The scalar information
            switch (t)
            {
                case "d":
                    if (reader.TokenType != JsonTokenType.Number)
                        return null;
                    r = reader.GetDouble();
                    reader.Read();
                    break;

                case "c":
                    if (reader.TokenType != JsonTokenType.Number)
                        return null;
                    double a = reader.GetDouble();
                    reader.Read();
                    if (reader.TokenType != JsonTokenType.Number)
                        return null;
                    double b = reader.GetDouble();
                    reader.Read();
                    r = new Complex(a, b);
                    break;
            }

            // End of the array
            if (reader.TokenType != JsonTokenType.EndArray)
                return null;
            reader.Read();
            return r;
        }
        private List<(string, int, int)>? ReadUnits(ref Utf8JsonReader reader)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
                return null;
            reader.Read();

            var list = new List<(string, int, int)>();
            while (reader.TokenType == JsonTokenType.StartArray)
            {
                reader.Read();

                // Unit name
                if (reader.TokenType != JsonTokenType.String)
                    return null;
                string? unit = reader.GetString();
                reader.Read();

                // Numerator
                if (unit is null || reader.TokenType != JsonTokenType.Number)
                    return null;
                int numerator = reader.GetInt32();
                reader.Read();

                // Denominator
                if (reader.TokenType != JsonTokenType.Number)
                    return null;
                int denominator = reader.GetInt32();
                reader.Read();

                // Store
                list.Add((unit, numerator, denominator));

                // End of the array
                if (reader.TokenType != JsonTokenType.EndArray)
                    return null;
                reader.Read();
            }

            if (reader.TokenType != JsonTokenType.EndArray)
                return null;
            reader.Read();

            return list;
        }

        public override void Write(Utf8JsonWriter writer, IQuantity value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            
            // Write the value depending on the type
            switch (value)
            {
                case Quantity<double> dbl:
                    writer.WriteStartArray("s");
                    writer.WriteStringValue("d");
                    writer.WriteNumberValue(dbl.Scalar);
                    writer.WriteEndArray();
                    writer.WriteStartArray("u");
                    foreach (var pair in dbl.Unit.Dimension)
                    {
                        writer.WriteStartArray();
                        writer.WriteStringValue(pair.Key);
                        writer.WriteNumberValue(pair.Value.Numerator);
                        writer.WriteNumberValue(pair.Value.Denominator);
                        writer.WriteEndArray();
                    }
                    writer.WriteEndArray();
                    break;

                case Quantity<Complex> cplx:
                    break;
            }

            writer.WriteEndObject();
        }
    }
}
