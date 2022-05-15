using BenchmarkDotNet.Running;

namespace SimpleEventSourcing.Benchmarking
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<BenchmarkSQLiteVsNHVsEFCore>();
        }
    }
}