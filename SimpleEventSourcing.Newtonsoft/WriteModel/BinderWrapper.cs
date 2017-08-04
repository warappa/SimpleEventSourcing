using System;

namespace SimpleEventSourcing.WriteModel
{
	public class BinderWrapper : Newtonsoft.Json.Serialization.DefaultSerializationBinder
    {
        public ISerializationBinder Binder { get; }

        public BinderWrapper(ISerializationBinder binder)
		{
            Binder = binder;
		}

		public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
		{
			typeName = Binder.BindToName(serializedType);
			assemblyName = null;
		}

		public override Type BindToType(string assemblyName, string typeName)
		{
			return Binder.BindToType(typeName);
		}
    }
}