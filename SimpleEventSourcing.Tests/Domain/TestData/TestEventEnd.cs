namespace SimpleEventSourcing.Tests
{
	public class TestEventEnd : IEventWithId
	{
		public TestEventEnd(string id)
		{
			this.Id = id;
		}

		public string Id { get; private set; }
	}

    public class TestEventUnknown : IEventWithId
    {
        public TestEventUnknown(string id)
        {
            this.Id = id;
        }

        public string Id { get; private set; }
    }
}