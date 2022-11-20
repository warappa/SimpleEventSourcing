﻿using BenchmarkDotNet.Running;

namespace SimpleEventSourcing.Benchmarking
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<BenchmarkWriteModelSQLiteVsNHVsEFCore>();
            //var summary = BenchmarkRunner.Run<BenchmarkReadModelSQLiteVsNHVsEFCore>();

            //var b = new BenchmarkReadModelSQLiteVsNHVsEFCore();
            //await b.BenchmarkSQLite().ConfigureAwait(false);
            //await b.BenchmarkEFCore().ConfigureAwait(false);
        }
    }
}
