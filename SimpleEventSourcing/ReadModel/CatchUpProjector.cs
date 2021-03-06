﻿using SimpleEventSourcing.Messaging;
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
        protected readonly IObserverFactory observerFactory;
        private readonly string projectorIdentifier;
        private readonly ICheckpointPersister checkpointPersister;
        private readonly IStorageResetter storageResetter;
        private IObserveRawStreamEntries observer;
        private IDisposable subscription;

        public CatchUpProjector(
            TState state,
            ICheckpointPersister checkpointPersister,
            IPersistenceEngine engine,
            IStorageResetter storageResetter,
            IObserverFactory observerFactory
            )
            : base(state)
        {
            this.checkpointPersister = checkpointPersister ?? throw new ArgumentNullException(nameof(checkpointPersister));
            this.storageResetter = storageResetter ?? throw new ArgumentNullException(nameof(storageResetter));
            this.engine = engine ?? throw new ArgumentNullException(nameof(engine));
            this.observerFactory = observerFactory ?? throw new ArgumentNullException(nameof(observerFactory));

            projectorIdentifier = checkpointPersister.GetProjectorIdentifier(typeof(TState));
        }

        public override async Task StartAsync()
        {
            var lastKnownCheckpointNumber = await checkpointPersister.LoadLastCheckpointAsync(projectorIdentifier);

            if (lastKnownCheckpointNumber == -1)
            {
                await storageResetter.ResetAsync(ControlsReadModelsAttribute.GetControlledReadModels(typeof(TState)));
            }

            observer = await observerFactory.CreateObserverAsync(lastKnownCheckpointNumber, StateModel.PayloadTypes);

            subscription = observer
                .Buffer(TimeSpan.FromSeconds(0.1))
                .Select(x => Observable.FromAsync(() => ProcessStreamEntries(x)))
                .Concat() //Ensure that the results are serialized
                .Subscribe(); //do what you will here with the results of the async method calls

            await observer.StartAsync();
        }

        public async Task<bool> PollNowAsync()
        {
            if (observer is null)
            {
                throw new InvalidOperationException("CatchupProjector is currently not observing - did you call StartAsync?");
            }

            return await observer.PollNowAsync();
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
                    projectorIdentifier,
                    requiredMessages[requiredMessages.Count - 1].CheckpointNumber);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                subscription?.Dispose();
                subscription = null;
                observer?.Dispose();
                observer = null;
            }

            base.Dispose(disposing);
        }
    }
}
