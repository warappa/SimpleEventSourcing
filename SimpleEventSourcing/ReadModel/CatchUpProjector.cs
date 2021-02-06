using SimpleEventSourcing.Messaging;
using SimpleEventSourcing.State;
using SimpleEventSourcing.Storage;
using SimpleEventSourcing.WriteModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace SimpleEventSourcing.ReadModel
{
    public class CatchUpProjector<TState> : Projector<TState>
        where TState : class, IEventSourcedState<TState>, new()
    {
        protected readonly IPersistenceEngine engine;
        protected readonly Poller poller;

        private readonly ICheckpointPersister checkpointPersister;
        private readonly IStorageResetter storageResetter;

        public CatchUpProjector(
            TState state,
            ICheckpointPersister checkpointPersister,
            IPersistenceEngine engine,
            IStorageResetter storageResetter,
            int interval = 100
            )
            : base(state)
        {
            this.checkpointPersister = checkpointPersister;
            this.storageResetter = storageResetter;

            this.engine = engine;

            poller = new Poller(engine, interval);
        }

        public override async Task<IDisposable> StartAsync()
        {
            var lastKnownCheckpointNumber = await checkpointPersister.LoadLastCheckpointAsync(typeof(TState).Name);

            if (lastKnownCheckpointNumber == -1)
            {
                await storageResetter.ResetAsync(ControlsReadModelsAttribute.GetControlledReadModels(typeof(TState)));
            }

            var observer = poller.ObserveFrom(lastKnownCheckpointNumber, StateModel.PayloadTypes);

            observer
                .Buffer(TimeSpan.FromSeconds(0.1))
                .Select(x => Observable.FromAsync(() => ProcessStreamEntries(x)))
                .Concat() //Ensure that the results are serialized
                .Subscribe(); //do what you will here with the results of the async method calls

            await observer.StartAsync();

            return observer;
        }

        private async Task ProcessStreamEntries(IList<IRawStreamEntry> streamEntries)
        {
            if (streamEntries.Count == 0)
            {
                return;
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var requiredMessages = streamEntries
                .Select(streamEntry => streamEntry.ToTypedMessage(engine.Serializer))
                .ToList();

            stopwatch.Stop();
            Debug.WriteLine($"deserialized {typeof(TState).Name} {requiredMessages.Count}x ({streamEntries.Count}): {stopwatch.ElapsedMilliseconds}ms");
            stopwatch.Restart();

            if (StateModel is IDbScopeAware scopeaware)
            {
                using (scopeaware.OpenScope())
                {
                    await ApplyMessagesAsync(requiredMessages);
                }
            }
            else
            {
                await ApplyMessagesAsync(requiredMessages);
            }

            stopwatch.Stop();
            Debug.WriteLine($"{typeof(TState).Name} {requiredMessages.Count}x ({streamEntries.Count}): {stopwatch.ElapsedMilliseconds}ms");

            requiredMessages.Clear();
        }

        private async Task ApplyMessagesAsync(List<IMessage> requiredMessages)
        {
            foreach (var message in requiredMessages)
            {
                StateModel = StateModel.Apply(message) ?? StateModel;
            }

            if (requiredMessages.Count > 0)
            {
                await checkpointPersister.SaveCurrentCheckpointAsync(
                    checkpointPersister.GetProjectorIdentifier(typeof(TState)),
                    requiredMessages[requiredMessages.Count - 1].CheckpointNumber);
            }
        }
    }
}
