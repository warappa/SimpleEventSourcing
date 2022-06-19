using System;

namespace SimpleEventSourcing.WriteModel
{
    public class DefaultInstanceProvider : IInstanceProvider
    {
        public T GetInstance<T>()
        {
            return (T)GetInstance(typeof(T));
        }

        public object GetInstance(Type type)
        {
            return Activator.CreateInstance(type);
        }
    }
}
