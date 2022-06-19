using System;

namespace Shop.Core.Domain.Shared
{
    public interface IBaseEvent
    {
        void SetDateTime(DateTime dateTime);
    }
}
