using NUnit.Framework;
using SimpleEventSourcing.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;

namespace SimpleEventSourcing.WriteModel.Tests
{
    public abstract class PersistenceEngineTestsBase : TransactedTest
    {
        protected PersistenceEngineTestsBase(TestsBaseConfig config, bool initialize = true)
        {
            this.config = config;
            this.initialize = initialize;
        }

        protected TestsBaseConfig config;

        protected ISerializer serializer;

        protected IPersistenceEngine persistenceEngine;

        protected IRawStreamEntry[] testEvents;
        private readonly bool initialize;

        protected override void BeforeFixtureTransaction()
        {
            config.WriteModel.EnsureWriteDatabase();
            config.ReadModel.EnsureReadDatabase();
            base.BeforeFixtureTransaction();
        }

        protected virtual void EarlySetup()
        {

        }

        [SetUp]
        public void TestSetup()
        {
            EarlySetup();

            if (initialize)
            {
                Initialize();

                SaveStreamEntry();
            }
        }

        [TearDown]
        public void TearDown()
        {
            config.ReadModel.CleanupReadDatabase();
            config.WriteModel.CleanupWriteDatabase();
        }

        [OneTimeSetUp]
        virtual protected void SetupFixture()
        {

        }

        [OneTimeTearDown]
        virtual protected void CleanupFixture()
        {

        }

        virtual protected void Initialize()
        {
            persistenceEngine = config.WriteModel.GetPersistenceEngine();

            persistenceEngine.InitializeAsync().Wait();

            serializer = persistenceEngine.Serializer;
        }

        protected string GetWithPrefix(string identifier, string group, string category)
        {
            return GetPrefix(group, category) + "-" + identifier;
        }

        protected string GetPrefix(string group, string category)
        {
            var prefix = string.IsNullOrWhiteSpace(group) ? "" : group + "-";
            prefix += string.IsNullOrWhiteSpace(category) ? "" : category + "-";

            return prefix;
        }


        protected IEnumerable<IRawStreamEntry> CreateTestData(string group, string category)
        {
            var prefix = GetPrefix(group, category);

            var entries = new List<IRawStreamEntry>();

            for (var i = 0; i < 4; i++)
            {
                entries.Add(config.WriteModel.GenerateRawStreamEntry());
            }

            var emtpyDictionary = new Dictionary<string, object>();
            var emtpyDictionarySerialized = serializer.Serialize(emtpyDictionary);

            entries[0].Group = group;
            entries[0].Category = category;
            entries[0].StreamName = "teststream A";
            entries[0].Headers = emtpyDictionarySerialized;
            entries[0].MessageId = $"{prefix}messageId A 1";
            entries[0].Payload = serializer.Serialize<object>(new TestEvent { Value = $"{prefix}test A 1" });
            entries[0].PayloadType = serializer.Binder.BindToName(typeof(TestEvent));
            entries[0].StreamRevision = 1;
            entries[0].DateTime = DateTime.UtcNow;


            entries[1].Group = group;
            entries[1].Category = category;
            entries[1].StreamName = "teststream A";
            entries[1].Headers = emtpyDictionarySerialized;
            entries[1].MessageId = $"{prefix}messageId A 2";
            entries[1].Payload = serializer.Serialize<object>(new TestEvent2 { Value2 = $"{prefix}test A 2" });
            entries[1].PayloadType = serializer.Binder.BindToName(typeof(TestEvent2));
            entries[1].StreamRevision = 2;
            entries[1].DateTime = DateTime.UtcNow;

            entries[2].Group = group;
            entries[2].Category = category;
            entries[2].StreamName = "teststream B";
            entries[2].Headers = emtpyDictionarySerialized;
            entries[2].MessageId = $"{prefix}messageId B 1";
            entries[2].Payload = serializer.Serialize<object>(new TestEvent { Value = $"{prefix}test B 1" });
            entries[2].PayloadType = serializer.Binder.BindToName(typeof(TestEvent));
            entries[2].StreamRevision = 1;
            entries[2].DateTime = DateTime.UtcNow;

            entries[3].Group = group;
            entries[3].Category = category;
            entries[3].StreamName = "teststream B";
            entries[3].Headers = emtpyDictionarySerialized;
            entries[3].MessageId = $"{prefix}messageId B 2";
            entries[3].Payload = serializer.Serialize<object>(new TestEvent2 { Value2 = $"{prefix}test B 2" });
            entries[3].PayloadType = serializer.Binder.BindToName(typeof(TestEvent2));
            entries[3].StreamRevision = 2;
            entries[3].DateTime = DateTime.UtcNow;

            return entries;
        }

        protected void SaveStreamEntry()
        {
            testEvents = CreateTestData(null, null)
                .Concat(CreateTestData("testgroup", null))
                .Concat(CreateTestData(null, "testcategory"))
                .Concat(CreateTestData("testgroup", "testcategory"))
                .ToArray();

            persistenceEngine.SaveStreamEntries(testEvents);

            FixTestData();
        }

        protected void FixTestData()
        {
            var rawStreamEntries = persistenceEngine.LoadStreamEntries().ToList();

            foreach (var entry in rawStreamEntries)
            {
                var testData = testEvents.First(x => x.MessageId == entry.MessageId);

                testData.CommitId = entry.CommitId;
                testData.CheckpointNumber = entry.CheckpointNumber;
                testData.StreamRevision = entry.StreamRevision;
            }
        }
    }
}
