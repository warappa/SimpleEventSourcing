using FluentAssertions;
using NUnit.Framework;
using SimpleEventSourcing.State;
using SimpleEventSourcing.WriteModel.InMemory.Tests;
using SimpleEventSourcing.WriteModel.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleEventSourcing.WriteModel.InMemory.WriteModel.Tests
{
    [TestFixture]
    public class PersistenceEngineBasicTests : PersistenceEngineTestsBase
    {
        public PersistenceEngineBasicTests()
            : base(new InMemoryTestConfig(), false)
        {
        }

        [Test]
        public async Task Can_initializeAsync()
        {
            await InitializeAsync().ConfigureAwait(false);
        }

        [Test]
        public async Task Can_save_streamEntriesAsync()
        {
            await InitializeAsync().ConfigureAwait(false);

            await SaveStreamEntryAsync().ConfigureAwait(false);
        }
    }
}
