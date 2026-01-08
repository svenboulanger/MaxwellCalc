using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MaxwellCalc.Core.Units;

/// <summary>
/// A factory method for creating <see cref="JsonConverter{T}"/> of <see cref="Quantity{T}"/>.
/// </summary>
public class QuantityJsonConverterFactory : JsonConverterFactory
{
    /// <inheritdoc />
    public override bool CanConvert(Type typeToConvert)
    {
        if (!typeToConvert.IsGenericType)
            return false;
        if (typeToConvert.GetGenericTypeDefinition() != typeof(Quantity<>))
            return false;
        return true;
    }

    /// <inheritdoc />
    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        // Create a JSON converter using reflection
        var scalarType = typeToConvert.GetGenericArguments()[0];
        var converterType = typeof(QuantityJsonConverter<>).MakeGenericType(scalarType);
        return (JsonConverter)Activator.CreateInstance(converterType);
    }

}
