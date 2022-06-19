using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shop.UI.Web.AspNetCore.Blazor.Client
{
    public class TimeSpanConverter : JsonConverter<TimeSpan>
    {
        public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return TimeSpan.Parse(reader.GetString(), CultureInfo.InvariantCulture);
        }

        public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(format: null, CultureInfo.InvariantCulture));
        }
    }
}
