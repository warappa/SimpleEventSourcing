using System;
using System.Threading.Tasks;

namespace SimpleEventSourcing.Storage
{
    public interface IStorageResetter
    {
        Task ResetAsync(Type[] entityTypes, bool justDrop = false);
    }
}
