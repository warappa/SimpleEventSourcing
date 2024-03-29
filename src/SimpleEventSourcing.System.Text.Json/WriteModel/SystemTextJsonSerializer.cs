﻿using SimpleEventSourcing.WriteModel;
using System;
using System.Text.Json;

namespace SimpleEventSourcing.System.Text.Json.WriteModel
{
    public class SystemTextJsonSerializer : ISerializer
    {
        private readonly JsonSerializerOptions settings;

        public SystemTextJsonSerializer(JsonSerializerOptions settings, ISerializationBinder binder)
        {
            this.settings = settings;
            Binder = binder;
        }

        public SystemTextJsonSerializer(ISerializationBinder binder)
            : this(CreateDefaultOptions(binder), binder)
        {

        }

        public ISerializationBinder Binder { get; private set; }

        public string Serialize(Type type, object obj)
        {
            if (obj == null)
            {
                return null;
            }

            return JsonSerializer.Serialize(obj, type, settings);
        }

        public string Serialize<T>(object obj)
        {
            if (obj == null)
            {
                return null;
            }

            return JsonSerializer.Serialize(obj, typeof(T), settings);
        }

        public string Serialize(object obj)
        {
            if (obj == null)
            {
                return null;
            }

            return JsonSerializer.Serialize(obj, settings);
        }

        public object Deserialize(Type type, string value)
        {
            if (value == null)
            {
                return null;
            }

            return JsonSerializer.Deserialize(value, type, settings);
        }

        public T Deserialize<T>(string value)
        {
            if (value == null)
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(value, settings);
        }

        private static JsonSerializerOptions CreateDefaultOptions(ISerializationBinder binder)
        {
            return new JsonSerializerOptions()
            {
                Converters = { new PolymorphicJsonConverterFactory() }
            };
        }
    }
}
