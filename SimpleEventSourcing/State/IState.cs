﻿using System;
using System.Threading.Tasks;

namespace SimpleEventSourcing.State
{
    public interface ISynchronousState
    {
        Type[] PayloadTypes { get; }
        object UntypedApply(object eventOrMessage);
    }
    public interface IAsyncState
    {
        Type[] PayloadTypes { get; }
        Task<object> UntypedApplyAsync(object eventOrMessage);
    }
}
