﻿using SimpleEventSourcing.ReadModel;

namespace SimpleEventSourcing.Tests.ReadModel.InMemory
{
    public class TestEntityB : ITestEntityB
    {
        object IReadModelBase.Id { get; set; } = 0;

        public virtual int Id { get => (int)(this as IReadModelBase).Id; set => (this as IReadModelBase).Id = value; }

        public virtual string Value { get; set; }
        public string Streamname { get; set; }
    }
}
