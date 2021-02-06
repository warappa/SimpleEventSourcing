﻿using NUnit.Framework;
using SimpleEventSourcing.Tests.Storage;

namespace SimpleEventSourcing.SQLite.WriteModel.Tests
{
    [TestFixture]
    public class StorageTests : StorageResetterTests
    {
        public StorageTests()
            : base(new SQLiteTestConfig())
        {

        }
    }
}
