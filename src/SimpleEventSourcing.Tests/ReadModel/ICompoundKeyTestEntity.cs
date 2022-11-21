using SimpleEventSourcing.ReadModel;
using System;

namespace SimpleEventSourcing.Tests.ReadModel
{
    public interface ICompoundKeyTestEntity : IStreamReadModel
    {
        Guid Key1 { get; set; }
        Guid Key2 { get; set; }
        string Value { get; set; }
    }
}
