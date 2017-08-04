using System;

namespace SimpleEventSourcing.WriteModel
{
    public interface ISerializer
    {
        string Serialize(Type type, object obj);
        string Serialize<T>(object obj);
        string Serialize(object obj);
        object Deserialize(string value);
        object Deserialize(Type type, string value);
        T Deserialize<T>(string value);

        ISerializationBinder Binder { get; }
    }
}
