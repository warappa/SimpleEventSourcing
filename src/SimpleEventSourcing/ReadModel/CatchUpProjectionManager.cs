﻿using SimpleEventSourcing.Messaging;
using SimpleEventSourcing.State;
using SimpleEventSourcing.Storage;
using SimpleEventSourcing.Utils;
using SimpleEventSourcing.WriteModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace SimpleEventSourcing.ReadModel
{
    public class CatchUpProjectionManager<TProjector> : ProjectionManager<TProjector>
        where TProjector : class, IProjector, new()
    {
        protected readonly IPersistenceEngine engine;
        protected readonly IObserverFactory observerFactory;
        private readonly string projectorIdentifier;
        private readonly ICheckpointPersister checkpointPersister;
        private readonly IStorageResetter storageResetter;
        private IObserveRawStreamEntries observer;
        private readonly List<string> payloadValues;
        private IDisposable subscription;

        public CatchUpProjectionManager(
            TProjector projector,
            ICheckpointPersister checkpointPersister,
            IPersistenceEngine engine,
            IStorageResetter storageResetter,
            IObserverFactory observerFactory
            )
            : base(projector)
        {
            this.checkpointPersister = checkpointPersister ?? throw new ArgumentNullException(nameof(checkpointPersister));
            this.storageResetter = storageResetter ?? throw new ArgumentNullException(nameof(storageResetter));
            this.engine = engine ?? throw new ArgumentNullException(nameof(engine));
            this.observerFactory = observerFactory ?? throw new ArgumentNullException(nameof(observerFactory));

            projectorIdentifier = checkpointPersister.GetProjectorIdentifier(typeof(TProjector));

            payloadValues = GetPayloadValues(Projector.PayloadTypes);
        }

        public override async Task ResetAsync()
        {
            await storageResetter.ResetAsync(ControlsReadModelsAttribute.GetControlledReadModels(typeof(TProjector))).ConfigureAwait(false);
            await checkpointPersister.ResetCheckpointAsync(projectorIdentifier).ConfigureAwait(false);
        }

        public override async Task StartAsync()
        {
            var lastKnownCheckpointNumber = await checkpointPersister.LoadLastCheckpointAsync(projectorIdentifier).ConfigureAwait(false);

            if (lastKnownCheckpointNumber == CheckpointDefaults.NoCheckpoint)
            {
                await storageResetter.ResetAsync(ControlsReadModelsAttribute.GetControlledReadModels(typeof(TProjector))).ConfigureAwait(false);
            }

            // TODO: Maybe optimize for rebuilding again by setting payloadTypes only to null, if the last read didn't yield any results
            //       Still, top checkpoint number must be available in CheckpointInfos table after all events are processed.
            observer = await observerFactory.CreateObserverAsync(lastKnownCheckpointNumber, payloadTypes: null).ConfigureAwait(false);

            subscription = observer
                .Buffer(TimeSpan.FromSeconds(0.1))
                .Select(x => Observable.FromAsync(() => ProcessStreamEntries(x)))
                .Concat() //Ensure that the results are serialized
                .Subscribe(); //do what you will here with the results of the async method calls

            await observer.StartAsync().ConfigureAwait(false);
        }

        public async Task<bool> PollNowAsync()
        {
            if (observer is null)
            {
                throw new InvalidOperationException("CatchupProjector is currently not observing - did you call StartAsync?");
            }

            return await observer.PollNowAsync().ConfigureAwait(false);
        }

        private async Task ProcessStreamEntries(IList<IRawStreamEntry> streamEntries)
        {
            if (streamEntries.Count == 0)
            {
                return;
            }

            //var stopwatch = new Stopwatch();
            //stopwatch.Start();

            var requiredMessages = streamEntries
                .Where(x => payloadValues.Contains(x.PayloadType))
                .ToTypedMessages(engine.Serializer)
                .ToList();

            //stopwatch.Stop();
            //Debug.WriteLine($"deserialized {typeof(TState).Name} {requiredMessages.Count}x ({streamEntries.Count}): {stopwatch.ElapsedMilliseconds}ms");
            //stopwatch.Restart();

            if (Projector is IDbScopeAware scopeaware)
            {
                using (scopeaware.OpenScope())
                {
                    await ApplyMessagesAsync(requiredMessages).ConfigureAwait(false);
                }
            }
            else
            {
                await ApplyMessagesAsync(requiredMessages).ConfigureAwait(false);
            }

            if (streamEntries.Count > 0)
            {
                await checkpointPersister.SaveCurrentCheckpointAsync(
                    projectorIdentifier,
                    streamEntries[streamEntries.Count - 1].CheckpointNumber)
                    .ConfigureAwait(false);
            }

            //stopwatch.Stop();
            //Debug.WriteLine($"{typeof(TState).Name} {requiredMessages.Count}x ({streamEntries.Count}): {stopwatch.ElapsedMilliseconds}ms");
        }

        private async Task ApplyMessagesAsync(List<IMessage> requiredMessages)
        {
            foreach (var message in requiredMessages)
            {
                var result = Projector.UntypedApply(message) ?? Projector;
                Projector = await StateExtensions.ExtractStateAsync<TProjector>(result).ConfigureAwait(false);
            }
        }

        private List<string> GetPayloadValues(Type[] payloadTypes)
        {
            if (payloadTypes != null &&
                payloadTypes.Length > 0)
            {
                return payloadTypes.Select(x => engine.Serializer.Binder.BindToName(x)).ToList();
            }

            return null;
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
