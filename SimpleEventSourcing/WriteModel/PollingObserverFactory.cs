using System;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleEventSourcing.WriteModel
{
    public sealed class PollingObserverFactory : IPollingObserverFactory
    {
        private readonly IPersistenceEngine persistenceEngine;

        private TimeSpan defaultInterval;

        public PollingObserverFactory(IPersistenceEngine persistenceEngine, TimeSpan defaultInterval = default)
        {
            this.persistenceEngine = persistenceEngine ?? throw new ArgumentNullException(nameof(persistenceEngine));
            this.defaultInterval = defaultInterval == default ? TimeSpan.FromMilliseconds(100) : defaultInterval;
        }

        public async Task<IObserveRawStreamEntries> CreateObserverAsync(int lastKnownCheckpointNumber = -1, Type[] payloadTypes = null)
        {
            return await CreateObserverAsync(default, lastKnownCheckpointNumber, payloadTypes);
        }
        public async Task<IObserveRawStreamEntries> CreateObserverAsync(TimeSpan interval, int lastKnownCheckpointNumber = -1, Type[] payloadTypes = null)
        {
            if (interval.TotalMilliseconds == 0)
            {
                interval = defaultInterval;
            }
            else if (interval.TotalMilliseconds <= 0)
            {
                throw new ArgumentException("MustBeGreaterThanZero");
            }

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
                intervalInMilliseconds = (int)Math.Ceiling(interval.TotalMilliseconds);
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
                return await DoPoll(true);
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

                    instant = await DoPoll(false);
                }
            }

            private async Task<bool> DoPoll(bool waitIfNecessary)
            {
                var hasResults = false;
                while (true)
                {
                    if (Interlocked.CompareExchange(ref isPolling, 1, 0) == 0)
                    {
                        try
                        {
                            //var stopwatch = new Stopwatch();
                            //stopwatch.Start();

                            var entries = persistenceEngine.LoadStreamEntriesAsync(lastKnownCheckpointNumber + 1, payloadTypes: payloadTypes, take: 10000).ConfigureAwait(false);

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

                        break;
                    }

                    if (!waitIfNecessary)
                    {
                        break;
                    }

                    await Task.Delay(10).ConfigureAwait(false);
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
