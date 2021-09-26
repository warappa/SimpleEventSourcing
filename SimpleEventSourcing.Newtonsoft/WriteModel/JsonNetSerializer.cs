using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Reflection;

namespace SimpleEventSourcing.WriteModel
{
    public class JsonNetSerializer : ISerializer
    {
        private readonly JsonSerializerSettings settings;

        public ISerializationBinder Binder => (settings.SerializationBinder as BinderWrapper).Binder;

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
            if (obj == null)
            {
                return null;
            }

            return JsonConvert.SerializeObject(obj, type, settings);
        }

        public string Serialize<T>(object obj)
        {
            if (obj == null)
            {
                return null;
            }

            return JsonConvert.SerializeObject(obj, typeof(T), settings);
        }

        public string Serialize(object obj)
        {
            if (obj == null)
            {
                return null;
            }

            return JsonConvert.SerializeObject(obj, settings);
        }

        public object Deserialize(string value)
        {
            if (value == null)
            {
                return null;
            }

            return JsonConvert.DeserializeObject(value, settings);
        }

        public object Deserialize(Type type, string value)
        {
            if (value == null)
            {
                return null;
            }

            return JsonConvert.DeserializeObject(value, type, settings);
        }

        public T Deserialize<T>(string value)
        {
            if (value == null)
            {
                return default;
            }

            return JsonConvert.DeserializeObject<T>(value, settings);
        }

        private static JsonSerializerSettings CreateDefaultSettings(ISerializationBinder binder)
        {
            return new JsonSerializerSettings()
            {
                SerializationBinder = new BinderWrapper(binder),
                ContractResolver = new NonPublicPropertiesResolver(),
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
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                TypeNameHandling = TypeNameHandling.Auto
            };
        }


        public class NonPublicPropertiesResolver : DefaultContractResolver
        {
            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                var prop = base.CreateProperty(member, memberSerialization);
                if (member is PropertyInfo pi)
                {
                    prop.Readable = (pi.GetMethod != null);
                    prop.Writable = (pi.SetMethod != null);
                }
                return prop;
            }
        }
    }
}
