﻿using System;

namespace SimpleEventSourcing.WriteModel
{
    public class BinderWrapper : Newtonsoft.Json.Serialization.ISerializationBinder
    {
        public ISerializationBinder Binder { get; }

        public BinderWrapper(ISerializationBinder binder)
        {
            Binder = binder;
        }

        public void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            typeName = Binder.BindToName(serializedType);
            assemblyName = null;
        }

        public Type BindToType(string assemblyName, string typeName)
        {
            return Binder.BindToType(typeName);
        }
    }
}
