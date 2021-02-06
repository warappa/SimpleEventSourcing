using FluentAssertions;
using NUnit.Framework;
using SimpleEventSourcing.Tests;
using System;
using System.Threading.Tasks;

namespace SimpleEventSourcing.ReadModel.Tests
{
    [TestFixture]
    public abstract class ReadRepositoryTests : TransactedTest
    {
        protected IReadRepository readRepository;
        protected TestsBaseConfig config;

        protected ReadRepositoryTests(TestsBaseConfig config)
        {
            this.config = config;
        }

        protected override async Task BeforeFixtureTransactionAsync()
        {
            await config.WriteModel.EnsureWriteDatabaseAsync();
            await config.ReadModel.EnsureReadDatabaseAsync();
        }

        [TearDown]
        public async Task TearDown()
        {
            await config.ReadModel.CleanupReadDatabaseAsync();
            await config.WriteModel.CleanupWriteDatabaseAsync();
        }

        [SetUp]
        public async Task Initialize()
        {
            readRepository = config.ReadModel.GetReadRepository();

            var readResetter = config.ReadModel.GetStorageResetter();
            await readResetter.ResetAsync(new[] { config.ReadModel.GetTestEntityA().GetType() });
        }

        [Test]
        public async Task Can_save_entity()
        {
            await readRepository.InsertAsync(config.ReadModel.GetTestEntityA());
        }

        [Test]
        public void Can_save_entity_by_id_twice()
        {
            var expected = config.ReadModel.GetTestEntityA();
            readRepository.InsertAsync(expected).Wait();
            var loaded = (ITestEntityA)readRepository.GetAsync(expected.GetType(), expected.Id).Result;

            loaded.Value.Should().Be(expected.Value);

            loaded.Value = "test2";
            readRepository.UpdateAsync(loaded).Wait();

            loaded = (ITestEntityA)readRepository.GetAsync(expected.GetType(), expected.Id).Result;
            loaded.Value.Should().Be("test2");
        }

        [Test]
        public async Task Can_save_and_load_entity_by_id()
        {
            var expected = config.ReadModel.GetTestEntityA();

            await readRepository.InsertAsync(expected).ConfigureAwait(false);

            var loaded = (ITestEntityA)(await readRepository.GetAsync(expected.GetType(), expected.Id).ConfigureAwait(false));

            loaded.Id.Should().Be(expected.Id);

            var getAsyncGeneric = readRepository.GetType().GetMethod("GetAsync", new[] { expected.Id.GetType() });

            var getAsync = getAsyncGeneric.MakeGenericMethod(loaded.GetType());

            var loaded2 = ((dynamic)((Task)getAsync.Invoke(readRepository, new object[] { expected.Id }))).Result;
            loaded.Id.Should().Be(loaded2.Id);
            loaded.Value.Should().Be(loaded2.Value);
        }

        [Test]
        public void Can_save_and_load_entity_by_streamname()
        {
            var expected = config.ReadModel.GetTestEntityA();
            expected.Streamname = Guid.NewGuid().ToString();

            readRepository.InsertAsync(expected).Wait();

            var loaded = (ITestEntityA)readRepository.GetByStreamnameAsync(expected.GetType(), expected.Streamname).Result;

            loaded.Id.Should().Be(expected.Id);

            var getAsyncGeneric = readRepository.GetType().GetMethod("GetByStreamnameAsync", new[] { expected.Streamname.GetType() });

            var getAsync = getAsyncGeneric.MakeGenericMethod(loaded.GetType());

            var loaded2 = ((dynamic)((Task)getAsync.Invoke(readRepository, new object[] { expected.Streamname }))).Result;
            loaded.Streamname.Should().Be(loaded2.Streamname);
            loaded.Value.Should().Be(loaded2.Value);
        }

        [Test]
        public async Task Can_delete_entity()
        {
            var expected = config.ReadModel.GetTestEntityA();
            expected.Streamname = Guid.NewGuid().ToString();

            await readRepository.InsertAsync(expected).ConfigureAwait(false);

            var loaded = (ITestEntityA)(await readRepository.GetByStreamnameAsync(expected.GetType(), expected.Streamname).ConfigureAwait(false));

            await readRepository.DeleteAsync(loaded).ConfigureAwait(false);

            (await readRepository.GetByStreamnameAsync(expected.GetType(), expected.Streamname).ConfigureAwait(false))
                .Should().Be(null);
        }
    }
}
