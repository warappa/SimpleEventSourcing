using System;

namespace SimpleEventSourcing.WriteModel
{
    public interface IInstanceProvider
    {
        T GetInstance<T>();
        object GetInstance(Type type);
    }
}
