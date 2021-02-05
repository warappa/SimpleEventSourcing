using System;
using System.Reactive;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace SimpleEventSourcing.WriteModel
{
    public sealed class Poller
    {
        private IPersistenceEngine persistenceEngine { get; }
        private readonly int interval;

        public Poller(IPersistenceEngine persistenceEngine, int interval = 5000)
        {
            if (persistenceEngine == null)
            {
                throw new ArgumentNullException(nameof(persistenceEngine));
            }
            if (interval <= 0)
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
            private readonly int interval;
            private readonly Subject<IRawStreamEntry> subject = new Subject<IRawStreamEntry>();
            private readonly Random random = new Random();
            private int lastKnownCheckpointNumber;
            private int isPolling;
            private CancellationTokenSource tokenSource;
            private Task pollLoopTask;
            private bool disposed;

            public PollingObserveRawStreamEntries(IPersistenceEngine persistenceEngine, int interval, int lastKnownCheckpointNumber = 0, Type[] payloadTypes = null)
            {
                this.persistenceEngine = persistenceEngine;
                this.lastKnownCheckpointNumber = lastKnownCheckpointNumber;
                this.interval = interval;
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

            public bool PollNow()
            {
                return DoPoll();
            }

            private async Task PollLoop()
            {
                bool instant = true;

                while (true)
                {
                    if (tokenSource.IsCancellationRequested)
                    {
                        return;
                    }

                    if (!instant)
                    {
                        await Task.Delay(random.Next(interval / 2 + 1, interval)).ConfigureAwait(false);
                    }

                    instant = DoPoll();
                }
            }

            private bool DoPoll()
            {
                var hasResults = false;
                if (Interlocked.CompareExchange(ref isPolling, 1, 0) == 0)
                {
                    try
                    {
                        //var stopwatch = new Stopwatch();
                        //stopwatch.Start();

                        var entries = persistenceEngine.LoadStreamEntries(lastKnownCheckpointNumber + 1, payloadTypes: payloadTypes, take: 10000);

                        foreach (var entry in entries.ToList())
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
