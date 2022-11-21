using Shop.Core.Domain.Articles;
using Shop.Core.Domain.Shared;
using Shop.ReadModel.Articles;
using Shop.UI.Web.AspNet.Commands.Articles;
using Shop.UI.Web.Shared.ReadModels.Articles;
using SimpleEventSourcing.State;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace Shop.UI.Web.AspNet.Controllers
{
    [RoutePrefix("api/articles")]
    public partial class ArticlesController : ApiController
    {
        public async Task<IEnumerable<ArticleViewModel>> Get()
        {
            return (await Program.readRepository.QueryAsync<ArticleViewModel>(x => true)).ToList();
        }

        public async Task<ArticleViewModel> Get(string id)
        {
            return await Program.readRepository.GetByStreamnameAsync<ArticleViewModel>(id);
        }

        [HttpGet]
        [Route("GetArticleActivationHistory")]
        public async Task<IEnumerable<ArticleActivationHistoryViewModel>> GetArticleActivationHistory(string id)
        {
            return await Program.readRepository.QueryAsync<ArticleActivationHistoryViewModel>(x => x.Streamname == id);
        }

        [HttpPost]
        [Route("CreateArticle")]
        public async Task CreateArticle(CreateArticle command)
        {
            var article = new Article(command.Id, command.Articlenumber, command.Description, new Money(command.PriceValue, command.PriceIsoCode ?? "EUR"));
            var checkpointNumber = await Program.repository.SaveAsync(article);

            await WaitForReadModelUpdate<ArticleReadModelState>(checkpointNumber);
        }

        [HttpPost]
        [Route("DeactivateArticle")]
        public async Task DeactivateArticle(DeactivateArticle command)
        {
            var article = await Program.repository.GetAsync<Article>(command.Id);

            article.Deactivate(command.Reason);

            var checkpointNumber = await Program.repository.SaveAsync(article);

            await WaitForReadModelUpdate<ArticleReadModelState>(checkpointNumber);
        }

        [HttpPost]
        [Route("ActivateArticle")]
        public async Task ActivateArticle(ActivateArticle command)
        {
            var article = await Program.repository.GetAsync<Article>(command.Id);

            article.Activate(command.Reason);

            var checkpointNumber = await Program.repository.SaveAsync(article);

            await WaitForReadModelUpdate<ArticleReadModelState>(checkpointNumber);
        }

        private async Task WaitForReadModelUpdate<TReadModelState>(int? checkpointNumber = null)
            where TReadModelState : IAsyncProjector
        {
            checkpointNumber ??= await Program.repository.GetCurrentCheckpointNumberAsync();
            await Program.checkpointPersister.WaitForCheckpointNumberAsync<TReadModelState>(checkpointNumber.Value);
        }
    }
}
