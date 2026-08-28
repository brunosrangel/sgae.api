using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sgae.Application.Common.JsonConverters;

/// <summary>
/// Conversor JSON flexível para Enums.
/// Converte de inteiros, strings numéricas ("0", "1") e nomes literais ("Presencial", "online"),
/// prevenindo erros de Bad Request por divergência de tipo na deserialização.
/// </summary>
public class FlexibleEnumConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsEnum || (Nullable.GetUnderlyingType(typeToConvert)?.IsEnum ?? false);
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var isNullable = Nullable.GetUnderlyingType(typeToConvert) != null;
        var enumType = Nullable.GetUnderlyingType(typeToConvert) ?? typeToConvert;

        var converterType = typeof(FlexibleEnumConverter<>).MakeGenericType(enumType);
        var converter = (JsonConverter)Activator.CreateInstance(converterType)!;

        if (isNullable)
        {
            var nullableConverterType = typeof(NullableFlexibleEnumConverter<>).MakeGenericType(enumType);
            return (JsonConverter)Activator.CreateInstance(nullableConverterType, converter)!;
        }

        return converter;
    }
}

public class FlexibleEnumConverter<T> : JsonConverter<T> where T : struct, Enum
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            var intVal = reader.GetInt32();
            if (Enum.IsDefined(typeof(T), intVal))
            {
                return (T)Enum.ToObject(typeof(T), intVal);
            }

            // Fallback para conversão direta se o int for válido
            return (T)Enum.ToObject(typeof(T), intVal);
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            var strVal = reader.GetString();
            if (string.IsNullOrWhiteSpace(strVal))
            {
                return default;
            }

            // Se for número em string: "0", "1", "2"
            if (int.TryParse(strVal, out var parsedInt))
            {
                return (T)Enum.ToObject(typeof(T), parsedInt);
            }

            // Se for o nome do enum ("Presencial", "Online", etc.)
            if (Enum.TryParse<T>(strVal, ignoreCase: true, out var parsedEnum))
            {
                return parsedEnum;
            }
        }

        return default;
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}

public class NullableFlexibleEnumConverter<T> : JsonConverter<T?> where T : struct, Enum
{
    private readonly FlexibleEnumConverter<T> _innerConverter;

    public NullableFlexibleEnumConverter(FlexibleEnumConverter<T> innerConverter)
    {
        _innerConverter = innerConverter;
    }

    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType == JsonTokenType.String && string.IsNullOrWhiteSpace(reader.GetString()))
        {
            return null;
        }

        return _innerConverter.Read(ref reader, typeof(T), options);
    }

    public override void Write(Utf8JsonWriter writer, T? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            _innerConverter.Write(writer, value.Value, options);
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}
