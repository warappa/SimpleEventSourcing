using FluentAssertions;
using NUnit.Framework;
using SimpleEventSourcing.ReadModel.Tests;
using SimpleEventSourcing.WriteModel.InMemory.Tests;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleEventSourcing.ReadModel.InMemory.Tests
{
    [TestFixture]
    public class ReadRepositoryInMemoryTests : ReadRepositoryTests
    {
        public ReadRepositoryInMemoryTests()
            : base(new InMemoryTestConfig())
        {
        }

        [Test]
        public async Task Reset_removes_all_entities_of_given_types()
        {
            await readRepository.InsertAsync(new TestAReadModel()).ConfigureAwait(false);
            await readRepository.InsertAsync(new TestBReadModel()).ConfigureAwait(false);

            (await readRepository.QueryAsync<TestAReadModel>(x => true).ConfigureAwait(false))
                .Any()
                .Should().Be(true);

            (await readRepository.QueryAsync<TestBReadModel>(x => true).ConfigureAwait(false))
                .Any()
                .Should().Be(true);

            await (readRepository as ReadRepository).ResetAsync(new[] { typeof(TestAReadModel) });

            (await readRepository.QueryAsync<TestAReadModel>(x => true).ConfigureAwait(false))
                .Any()
                .Should().Be(false);
            (await readRepository.QueryAsync<TestBReadModel>(x => true).ConfigureAwait(false))
                .Any()
                .Should().Be(true);
        }

        public class TestAReadModel : IReadModel<int>
        {
            public int Id { get; set; }
            object IReadModelBase.Id { get => Id; set => Id = (int)value; }
        }

        public class TestBReadModel : IReadModel<int>
        {
            public int Id { get; set; }
            object IReadModelBase.Id { get => Id; set => Id = (int)value; }
        }
    }
}
