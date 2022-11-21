using SimpleEventSourcing.WriteModel;
using System;

namespace SimpleEventSourcing.Newtonsoft.WriteModel
{
    public class BinderWrapper : global::Newtonsoft.Json.Serialization.ISerializationBinder
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
