using SimpleEventSourcing.ReadModel;
using System.Threading.Tasks;

namespace SimpleEventSourcing.UI.ConsoleUI
{
    [ControlsReadModels(new[] { typeof(PersistentEntity) })]
    public class PersistentProjector : ReadRepositoryProjector<PersistentProjector>
    {
        public string Name { get; protected set; }
        public string SomethingDone { get; protected set; }
        public int Count { get; set; }

        public PersistentProjector() : base() { }
        public PersistentProjector(IReadRepository readRepository)
            : base(readRepository)
        {
        }

        public async Task Apply(TestAggregateCreated @event)
        {
            Count++;
            var s = new PersistentEntity
            {
                Streamname = @event.Id,
                Name = @event.Name
            };

            await readRepository.InsertAsync(s).ConfigureAwait(false);
        }

        public async Task Apply(SomethingDone @event)
        {
            Count++;
            await UpdateByIdAsync<PersistentEntity, string>(@event.Id,
                model =>
                {
                    model.Name += ".";
                }).ConfigureAwait(false);
        }

        public async Task Apply(Renamed @event)
        {
            Count++;
            await UpdateByIdAsync<PersistentEntity, string>(@event.Id,
                model =>
                {
                    model.Name = @event.NewName;
                }).ConfigureAwait(false);
        }
        /*
        protected override PersistentState InvokeAssociatedApply(object eventOrMessage)
        {
            if (eventOrMessage is IMessage msg)
            {
                eventOrMessage = msg.Body;
            }

            if (eventOrMessage is TestAggregateCreated created)
            {
                return Apply(created).ExtractState<PersistentState>();
            }
            if (eventOrMessage is SomethingDone done)
            {
                return Apply(done).ExtractState<PersistentState>();
            }
            if (eventOrMessage is Renamed renamed)
            {
                return Apply(renamed).ExtractState<PersistentState>();
            }

            return this;
        }*/
    }
}
