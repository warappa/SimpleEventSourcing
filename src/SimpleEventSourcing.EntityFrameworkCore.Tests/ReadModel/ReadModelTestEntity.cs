using SimpleEventSourcing.ReadModel;
using System;

namespace SimpleEventSourcing.EntityFrameworkCore.Tests.ReadModel
{

    public class ReadModelTestEntity : IReadModel<Guid>
    {
        public Guid Id { get; set; }
        public string Value { get; set; }

        object IReadModelBase.Id { get => Id; set => Id = (Guid)value; }
    }
}
