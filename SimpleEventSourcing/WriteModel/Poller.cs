using System;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleEventSourcing.WriteModel
{
    public sealed class Poller : IPoller
    {
        private IPersistenceEngine persistenceEngine { get; }
        private readonly TimeSpan interval;

        public Poller(IPersistenceEngine persistenceEngine)
            : this(persistenceEngine, TimeSpan.FromSeconds(5))
        {

        }
        public Poller(IPersistenceEngine persistenceEngine, TimeSpan interval)
        {
            if (persistenceEngine == null)
            {
                throw new ArgumentNullException(nameof(persistenceEngine));
            }
            if (interval.TotalMilliseconds <= 0)
            {
                throw new ArgumentException("MustBeGreaterThanZero");
            }

            this.persistenceEngine = persistenceEngine;
            this.interval = interval;
        }

        public IObserveRawStreamEntries ObserveFrom(int lastKnownCheckpointNumber = -1, Type[] payloadTypes = null)
        {
            return new PollingObserveRawStreamEntries(persistenceEngine, interval, lastKnownCheckpointNumber, payloadTypes);
        }

        private class PollingObserveRawStreamEntries : IObserveRawStreamEntries
        {
            private readonly IPersistenceEngine persistenceEngine;
            private readonly Type[] payloadTypes;
            private readonly int intervalInMilliseconds;
            private readonly Subject<IRawStreamEntry> subject = new();
            private readonly Random random = new();
            private int lastKnownCheckpointNumber;
            private int isPolling;
            private CancellationTokenSource tokenSource;
            private Task pollLoopTask;
            private bool disposed;

            public PollingObserveRawStreamEntries(IPersistenceEngine persistenceEngine, TimeSpan interval, int lastKnownCheckpointNumber = 0, Type[] payloadTypes = null)
            {
                this.persistenceEngine = persistenceEngine;
                this.lastKnownCheckpointNumber = lastKnownCheckpointNumber;
                this.intervalInMilliseconds = (int)Math.Ceiling(interval.TotalMilliseconds);
                this.payloadTypes = payloadTypes;
            }

            public IDisposable Subscribe(IObserver<IRawStreamEntry> observer)
            {
                return subject.Subscribe(observer);
            }

            public async Task StartAsync()
            {
                if (tokenSource != null)
                {
                    tokenSource.Cancel();
                }

                tokenSource = new CancellationTokenSource();

                pollLoopTask = Task.Run(async () => await PollLoop());
            }

            public async Task<bool> PollNowAsync()
            {
                return await DoPoll();
            }

            private async Task PollLoop()
            {
                var instant = true;

                while (true)
                {
                    if (tokenSource.IsCancellationRequested)
                    {
                        return;
                    }

                    if (!instant)
                    {
                        
                        await Task.Delay(random.Next(intervalInMilliseconds / 2 + 1, intervalInMilliseconds)).ConfigureAwait(false);
                    }

                    instant = await DoPoll();
                }
            }

            private async Task<bool> DoPoll()
            {
                var hasResults = false;
                if (Interlocked.CompareExchange(ref isPolling, 1, 0) == 0)
                {
                    try
                    {
                        //var stopwatch = new Stopwatch();
                        //stopwatch.Start();

                        var entries = persistenceEngine.LoadStreamEntriesAsync(lastKnownCheckpointNumber + 1, payloadTypes: payloadTypes, take: 10000);

                        await foreach (var entry in entries)
                        {
                            if (tokenSource.IsCancellationRequested)
                            {
                                Interlocked.Exchange(ref isPolling, 0);

                                return false;
                            }

                            hasResults = true;

                            subject.OnNext(entry);

                            lastKnownCheckpointNumber = entry.CheckpointNumber;
                        }
                    }
                    catch
                    {
                        // These exceptions are expected to be transient
                    }

                    Interlocked.Exchange(ref isPolling, 0);
                }

                return hasResults;
            }

            public void Dispose()
            {
                Dispose(true);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (disposed)
                {
                    return;
                }

                if (disposing)
                {
                    disposed = true;

                    tokenSource?.Cancel();

                    subject.OnCompleted();
                    subject.Dispose();

                    if (Interlocked.CompareExchange(ref isPolling, 1, 0) == 1)
                    {
                        pollLoopTask?.Wait(5000);
                    }
                }
            }
        }
    }
}
