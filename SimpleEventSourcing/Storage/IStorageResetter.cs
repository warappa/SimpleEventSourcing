using System;

namespace SimpleEventSourcing.Storage
{
    public interface IStorageResetter
    {
        void Reset(Type[] entityTypes, bool justDrop = false);
    }
}
