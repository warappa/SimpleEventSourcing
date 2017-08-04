using Newtonsoft.Json;
using System;

namespace SimpleEventSourcing.WriteModel
{
    public class JsonNetSerializer : ISerializer
    {
        private readonly JsonSerializerSettings settings;

        public ISerializationBinder Binder => (settings.Binder as BinderWrapper).Binder;

        public JsonNetSerializer(JsonSerializerSettings settings)
        {
            this.settings = settings;
        }

        public JsonNetSerializer(ISerializationBinder binder)
            : this(CreateDefaultSettings(binder))
        {

        }

        public string Serialize(Type type, object obj)
        {
            return JsonConvert.SerializeObject(obj, type, settings);
        }
        public string Serialize<T>(object obj)
        {
            return JsonConvert.SerializeObject(obj, typeof(T), settings);
        }

        public string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj, settings);
        }

        public object Deserialize(string value)
        {
            return JsonConvert.DeserializeObject(value, settings);
        }

        public object Deserialize(Type type, string value)
        {
            return JsonConvert.DeserializeObject(value, type, settings);
        }

        public T Deserialize<T>(string value)
        {
            return JsonConvert.DeserializeObject<T>(value, this.settings);
        }

        private static JsonSerializerSettings CreateDefaultSettings(ISerializationBinder binder)
        {
            return new JsonSerializerSettings()
            {
                Binder = new BinderWrapper(binder),
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateParseHandling = DateParseHandling.DateTime,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                DefaultValueHandling = DefaultValueHandling.Include,
                FloatFormatHandling = FloatFormatHandling.DefaultValue,
                FloatParseHandling = FloatParseHandling.Double,
                Formatting = Formatting.None,
                MetadataPropertyHandling = MetadataPropertyHandling.Default,
                MissingMemberHandling = MissingMemberHandling.Error,
                NullValueHandling = NullValueHandling.Include,
                ObjectCreationHandling = ObjectCreationHandling.Auto,
                PreserveReferencesHandling = PreserveReferencesHandling.None,
                ReferenceLoopHandling = ReferenceLoopHandling.Error,
                StringEscapeHandling = StringEscapeHandling.Default,
                TypeNameAssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple,
                TypeNameHandling = TypeNameHandling.Auto
            };
        }
    }
}
