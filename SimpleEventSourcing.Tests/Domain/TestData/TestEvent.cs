namespace SimpleEventSourcing.Tests
{
	public class TestEvent : IEventWithId
	{
		public TestEvent(string id, string value)
		{
			this.Id = id;
			this.Value = value;
		}

		public string Id { get; private set; }
		public string Value { get; private set; }
	}
}