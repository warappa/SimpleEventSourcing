﻿using FluentAssertions;
using NUnit.Framework;
using SimpleEventSourcing.SQLite.Tests.Storage;
using SimpleEventSourcing.Tests.ReadModel;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleEventSourcing.SQLite.Tests.ReadModel
{
    [TestFixture]
    public class ReadRepositorySQLiteTests : ReadRepositoryTests
    {
        public ReadRepositorySQLiteTests()
            : base(new SQLiteTestConfig())
        {
        }

        [Test]
        public async Task Can_save_and_load_entity_by_id_with_textblob()
        {
            var expected = (TestEntityA)config.ReadModel.GetTestEntityA();

            await readRepository.InsertAsync(expected).ConfigureAwait(false);

            var loaded = (TestEntityA)await readRepository.GetAsync(expected.GetType(), expected.Id).ConfigureAwait(false);

            loaded.Id.Should().Be(expected.Id);
            loaded.SubData.Should().NotBeNull();
            loaded.SubData.PropA.Should().Be(expected.SubData.PropA);
            loaded.SubData.PropB.Should().Be(expected.SubData.PropB);

            var getAsyncGeneric = readRepository.GetType().GetMethod("GetAsync", new[] { expected.Id.GetType() });

            var getAsync = getAsyncGeneric.MakeGenericMethod(loaded.GetType());

            var loaded2 = (TestEntityA)((dynamic)getAsync.Invoke(readRepository, new object[] { expected.Id })).Result;
            loaded2.Id.Should().Be(loaded.Id);
            loaded2.Value.Should().Be(loaded.Value);
            loaded2.SubData.Should().NotBeNull();
            loaded2.SubData.PropA.Should().Be(expected.SubData.PropA);
            loaded2.SubData.PropB.Should().Be(expected.SubData.PropB);

            var loaded3 = (await readRepository.QueryAsync(typeof(TestEntityA), x => true).ConfigureAwait(false))
                .Cast<TestEntityA>()
                .FirstOrDefault();

            loaded3.SubData.Should().NotBeNull();
            loaded3.SubData.PropA.Should().Be(expected.SubData.PropA);
            loaded3.SubData.PropB.Should().Be(expected.SubData.PropB);
        }
    }
}
