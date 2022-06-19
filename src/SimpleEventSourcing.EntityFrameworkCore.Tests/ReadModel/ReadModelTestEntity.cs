using SimpleEventSourcing.ReadModel;
using System;

namespace SimpleEventSourcing.EntityFrameworkCore.Tests
{

    public class ReadModelTestEntity : IReadModel<Guid>
    {
        public Guid Id { get; set; }
        public string Value { get; set; }

        object IReadModelBase.Id { get { return Id; } set { Id = (Guid)value; } }
    }
}