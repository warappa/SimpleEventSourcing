using SimpleEventSourcing.ReadModel;
using System.Threading.Tasks;

namespace SimpleEventSourcing.UI.ConsoleCore
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

        public async ValueTask Apply(Renamed @event)
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
