using SimpleEventSourcing.Messaging;
using SimpleEventSourcing.WriteModel;
using System;

namespace SimpleEventSourcing.Tests.Domain.TestData
{
    public class TestProcessManagerRouter : ProcessManagerRouter
    {
        public TestProcessManagerRouter(IProcessManagerRepository processManagerRepository, Func<IMessage, string> processIdExtractor)
            : base(processManagerRepository, processIdExtractor)
        {

        }
    }
}
