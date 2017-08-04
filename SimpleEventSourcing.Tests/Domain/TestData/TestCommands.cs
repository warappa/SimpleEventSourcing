using SimpleEventSourcing.Messaging;

namespace SimpleEventSourcing.Tests.Domain.TestData
{
	public class TestCommandsOne : ICommand
	{
		public TestCommandsOne(string id, string name)
		{
			this.Id = id;
			this.Name = name;
		}

		public string Id { get; set; }
		public string Name { get; set; }
	}
}
