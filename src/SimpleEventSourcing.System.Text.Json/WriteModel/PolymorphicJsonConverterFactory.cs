using System;
using System.Collections;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SimpleEventSourcing.System.Text.Json.WriteModel
{
    internal class PolymorphicJsonConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert.IsAbstract && !typeof(IEnumerable).IsAssignableFrom(typeToConvert);
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            return (JsonConverter)Activator.CreateInstance(typeof(PolymorphicConverter<>).MakeGenericType(typeToConvert));
        }
    }
}
