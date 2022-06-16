using System.Threading.Tasks;

namespace SimpleEventSourcing.ReadModel.Tests
{
    [ControlsReadModels(new[] { typeof(CatchUpReadModel) })]
    public class CatchUpStateWithReadModel : ReadRepositoryProjector<CatchUpStateWithReadModel>
    {
        private int idcounter = 0;

        public CatchUpStateWithReadModel()
            : base()
        {

        }
        public CatchUpStateWithReadModel(IReadRepository readRepository)
            : base(readRepository)
        {

        }

        public async ValueTask<CatchUpStateWithReadModel> Apply(TestCatchUpEvent @event)
        {
            var found = false;
            await QueryAndUpdateAsync<CatchUpReadModel>(x => true, model =>
            {
                found = true;
                model.Count++;
            })
            .ConfigureAwait(false);

            if (!found)
            {
                await readRepository.InsertAsync(new CatchUpReadModel
                {
                    Id = ++idcounter,
                    Count = 1
                })
                .ConfigureAwait(false);
            }

            return this;
        }
    }
}
