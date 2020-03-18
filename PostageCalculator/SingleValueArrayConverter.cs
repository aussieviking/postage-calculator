using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PostageCalculator
{
    public class SingleValueArrayConverter<T> : JsonConverter<ICollection<T>> where T : class, new()
    {
        public override void Write(
            Utf8JsonWriter writer,
            ICollection<T> value,
            JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value.Count == 1 ? (object)value.Single() : value);
        }

        public override ICollection<T> Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            return reader.TokenType switch
            {
                JsonTokenType.StartObject => new Collection<T> { JsonSerializer.Deserialize<T>(ref reader) },
                JsonTokenType.StartArray => JsonSerializer.Deserialize<ICollection<T>>(ref reader),
                _ => throw new ArgumentOutOfRangeException($"Converter does not support JSON token type {reader.TokenType}.")
            };
        }
    }
}