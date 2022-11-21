using FluentAssertions;
using NUnit.Framework;
using SimpleEventSourcing.ReadModel;
using SimpleEventSourcing.Tests.WriteModel;
using System;
using System.Threading.Tasks;

namespace SimpleEventSourcing.Tests.ReadModel
{
    [TestFixture]
    public abstract class ReadRepositoryWithCompoundKeyEntityFrameworkTestsBase : TransactedTest
    {
        protected IReadRepository readRepository;
        protected TestsBaseConfig config;

        protected ReadRepositoryWithCompoundKeyEntityFrameworkTestsBase(TestsBaseConfig config)
        {
            this.config = config;
        }

        protected override async Task BeforeFixtureTransactionAsync()
        {
            await config.WriteModel.EnsureWriteDatabaseAsync().ConfigureAwait(false);
            await config.ReadModel.EnsureReadDatabaseAsync().ConfigureAwait(false);
        }

        [TearDown]
        public async Task TearDown()
        {
            await config.ReadModel.CleanupReadDatabaseAsync().ConfigureAwait(false);
            await config.WriteModel.CleanupWriteDatabaseAsync().ConfigureAwait(false);
        }

        [SetUp]
        public async Task Initialize()
        {
            readRepository = config.ReadModel.GetReadRepository();

            var readResetter = config.ReadModel.GetStorageResetter();
            await readResetter.ResetAsync(new[] { config.ReadModel.GetCompoundKeyTestEntity().GetType() }).ConfigureAwait(false);
        }

        [Test]
        public async Task Can_save_entity()
        {
            await readRepository.InsertAsync(config.ReadModel.GetCompoundKeyTestEntity()).ConfigureAwait(false);
        }

        [Test]
        public async Task Can_save_entity_by_id_twice()
        {
            var expected = config.ReadModel.GetCompoundKeyTestEntity();
            readRepository.InsertAsync(expected).Wait();
            var loaded = (ICompoundKeyTestEntity)await readRepository.GetAsync(expected.GetType(), new[] { expected.Key1, expected.Key2 }).ConfigureAwait(false);

            loaded.Value.Should().Be(expected.Value);

            loaded.Value = "test2";
            readRepository.UpdateAsync(loaded).Wait();

            loaded = (ICompoundKeyTestEntity)await readRepository.GetAsync(expected.GetType(), new[] { expected.Key1, expected.Key2 }).ConfigureAwait(false);
            loaded.Value.Should().Be("test2");
        }

        [Test]
        public async Task Can_save_and_load_entity_by_streamname()
        {
            var expected = config.ReadModel.GetCompoundKeyTestEntity();
            expected.Key1 = Guid.NewGuid();
            expected.Key2 = Guid.NewGuid();

            readRepository.InsertAsync(expected).Wait();

            var loaded = (ICompoundKeyTestEntity)await readRepository.GetByStreamnameAsync(expected.GetType(), expected.Streamname).ConfigureAwait(false);

            loaded.Id.Should().Be(expected.Id);

            var getAsyncGeneric = readRepository.GetType().GetMethod("GetByStreamnameAsync", new[] { expected.Streamname.GetType() });

            var getAsync = getAsyncGeneric.MakeGenericMethod(loaded.GetType());

            var loaded2 = ((dynamic)(Task)getAsync.Invoke(readRepository, new object[] { expected.Streamname })).Result;
            loaded.Streamname.Should().Be(loaded2.Streamname);
            loaded.Value.Should().Be(loaded2.Value);
        }

        [Test]
        public async Task Can_delete_entity()
        {
            var expected = config.ReadModel.GetCompoundKeyTestEntity();
            expected.Key1 = Guid.NewGuid();
            expected.Key2 = Guid.NewGuid();

            await readRepository.InsertAsync(expected).ConfigureAwait(false);

            var loaded = (ICompoundKeyTestEntity)await readRepository.GetByStreamnameAsync(expected.GetType(), expected.Streamname).ConfigureAwait(false);

            await readRepository.DeleteAsync(loaded).ConfigureAwait(false);

            (await readRepository.GetByStreamnameAsync(expected.GetType(), expected.Streamname).ConfigureAwait(false))
                .Should().Be(null);
        }
    }
}
