using SimpleEventSourcing.Benchmarking.Domain;
using SimpleEventSourcing.ReadModel;

namespace SimpleEventSourcing.Benchmarking.ReadModel
{
    [ControlsReadModels(new[] { typeof(PersistentEntity) })]
    public class PersistentState : ReadRepositoryProjector<PersistentState>
    {
        public string Name { get; protected set; }
        public string SomethingDone { get; protected set; }
        public int Count { get; set; }

        public PersistentState() : base() { }
        public PersistentState(IReadRepository readRepository)
            : base(readRepository)
        {
        }

        public async ValueTask Apply(TestAggregateCreated @event)
        {
            Count++;
            var s = new PersistentEntity
            {
                Streamname = @event.Id,
                Name = @event.Name
            };

            await readRepository.InsertAsync(s).ConfigureAwait(false);
        }

        public async ValueTask Apply(SomethingDone @event)
        {
            Count++;
            await UpdateByIdAsync<PersistentEntity, string>(@event.Id,
                model =>
                {
                    model.Name += ".";
                }).ConfigureAwait(false);
        }

        public async ValueTask Apply(SomethingSpecialDone @event)
        {
            Count++;
            await UpdateByIdAsync<PersistentEntity, string>(@event.Id,
                model =>
                {
                    model.Name += ".";
                }).ConfigureAwait(false);
        }

        public async ValueTask Apply(Renamed @event)
        {
            Count++;
            await UpdateByIdAsync<PersistentEntity, string>(@event.Id,
                model =>
                {
                    model.Name = @event.NewName;
                }).ConfigureAwait(false);
        }
    }
}
