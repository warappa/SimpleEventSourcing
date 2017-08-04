using FluentAssertions;
using NUnit.Framework;
using SimpleEventSourcing.Domain;
using SimpleEventSourcing.Messaging;
using SimpleEventSourcing.State;
using System;
using System.Linq;

namespace SimpleEventSourcing.Tests.Domain
{
    [TestFixture]
    public class EventSourcedEntityTests
    {
        private IEvent[] testEvents;

        [SetUp]
        public void Initialize()
        {
            testEvents = new IEvent[] {
                new ParentCreated("id", "name", DateTime.UtcNow),
                new ParentRenamed("id", "newname", DateTime.UtcNow)
            };
        }

        [Test]
        public void Load_by_constructor_should_be_the_same_as_loading_be_LoadEvents()
        {
            var loadedByConstructor = new EventSourcedEntity<TestState, string>(testEvents);

            var loadedByLoadEvents = new EventSourcedEntity<TestState, string>(Enumerable.Empty<IEvent>());
            loadedByLoadEvents.AsIEventSourcedEntity().LoadEvents(testEvents);

            loadedByLoadEvents.Should().Be(loadedByConstructor);
        }

        [Test]
        public void Loaded_events_dont_count_as_uncommitted_events()
        {
            var loadedByConstructor = new EventSourcedEntity<TestState, string>(testEvents);

            var loadedByLoadEvents = new EventSourcedEntity<TestState, string>(Enumerable.Empty<IEvent>());
            loadedByLoadEvents.AsIEventSourcedEntity().LoadEvents(testEvents);

            loadedByConstructor.AsIEventSourcedEntity().UncommittedEvents.Count().Should().Be(0);

            loadedByLoadEvents.AsIEventSourcedEntity().UncommittedEvents.Count().Should().Be(0);
        }

        [Test]
        public void Load_by_constructor_and_initial_state_should_be_the_same_as_loading_be_LoadEvents_and_initial_state()
        {
            var loadedByConstructor = new EventSourcedEntity<TestState, string>(testEvents.Take(1), new TestState().Apply(testEvents[1]));

            var loadedByLoadEvents = new EventSourcedEntity<TestState, string>(Enumerable.Empty<IEvent>());
            loadedByLoadEvents.AsIEventSourcedEntity().LoadEvents(testEvents.Take(1), new TestState().Apply(testEvents[1]));

            loadedByLoadEvents.Should().Be(loadedByConstructor);
        }

        public class TestState : StreamState<TestState>
        {
            public string Name { get; private set; }

            public TestState Apply(ParentCreated @event)
            {
                StreamName = @event.Id;
                Name = @event.Name;

                return this;
            }

            public TestState Apply(ParentRenamed @event)
            {
                Name = @event.Name;

                return this;
            }
        }
    }
}
