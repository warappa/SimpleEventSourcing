using System;

namespace SimpleEventSourcing.WriteModel
{
    public interface ISerializationBinder
    {
        string BindToName(Type type);
        Type BindToType(string typename);
    }
}
