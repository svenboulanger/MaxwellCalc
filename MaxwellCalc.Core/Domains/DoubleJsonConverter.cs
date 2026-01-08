using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MaxwellCalc.Core.Domains;

/// <summary>
/// A <see cref="JsonConverter{T}"/> that works for a <see cref="DoubleDomain"/>.
/// </summary>
public class DoubleJsonConverter : JsonConverter<double>
{
    /// <inheritdoc />
    public override double Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => reader.GetDouble();

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, double value, JsonSerializerOptions options) => writer.WriteNumberValue(value);
}
