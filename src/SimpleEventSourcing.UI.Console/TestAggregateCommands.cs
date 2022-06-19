using SimpleEventSourcing.Messaging;

namespace SimpleEventSourcing.UI.ConsoleUI
{
    public class TestAggregateRename : IEventSourcedEntityCommand
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class TestAggregateDoSomething : IEventSourcedEntityCommand
    {
        public string Id { get; set; }
        public string Foo { get; set; }
    }
}
