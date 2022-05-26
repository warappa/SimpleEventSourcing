using SimpleEventSourcing.Messaging;
using SimpleEventSourcing.State;
using SimpleEventSourcing.Storage;
using SimpleEventSourcing.Utils;
using SimpleEventSourcing.WriteModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        }

        public override async Task ResetAsync()
        {
            await storageResetter.ResetAsync(ControlsReadModelsAttribute.GetControlledReadModels(typeof(TProjector)));
            await checkpointPersister.ResetCheckpointAsync(projectorIdentifier);
        }

        public override async Task StartAsync()
        {
            var lastKnownCheckpointNumber = await checkpointPersister.LoadLastCheckpointAsync(projectorIdentifier);

            if (lastKnownCheckpointNumber == CheckpointDefaults.NoCheckpoint)
            {
                await storageResetter.ResetAsync(ControlsReadModelsAttribute.GetControlledReadModels(typeof(TProjector)));
            }

            observer = await observerFactory.CreateObserverAsync(lastKnownCheckpointNumber, Projector.PayloadTypes);

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

            //var stopwatch = new Stopwatch();
            //stopwatch.Start();

            var requiredMessages = streamEntries
                .Select(streamEntry => streamEntry.ToTypedMessage(engine.Serializer))
                .ToList();

            //stopwatch.Stop();
            //Debug.WriteLine($"deserialized {typeof(TState).Name} {requiredMessages.Count}x ({streamEntries.Count}): {stopwatch.ElapsedMilliseconds}ms");
            //stopwatch.Restart();

            if (Projector is IDbScopeAware scopeaware)
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

            if (requiredMessages.Count > 0)
            {
                await checkpointPersister.SaveCurrentCheckpointAsync(
                    projectorIdentifier,
                    requiredMessages[requiredMessages.Count - 1].CheckpointNumber);
            }

            //stopwatch.Stop();
            //Debug.WriteLine($"{typeof(TState).Name} {requiredMessages.Count}x ({streamEntries.Count}): {stopwatch.ElapsedMilliseconds}ms");

            requiredMessages.Clear();
        }

        private async Task ApplyMessagesAsync(List<IMessage> requiredMessages)
        {
            foreach (var message in requiredMessages)
            {
                var result = Projector.UntypedApply(message) ?? Projector;
                Projector = await StateExtensions.ExtractStateAsync<TProjector>(result);
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
