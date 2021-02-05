using Shop.Core.Domain.Articles;
using Shop.Core.Domain.Shared;
using Shop.ReadModel.Articles;
using Shop.Web.UI.Commands.Articles;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace Shop.Web.UI.Controllers
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
        public Task CreateArticle(CreateArticle command)
        {
            var article = new Article(command.Id, command.Articlenumber, command.Description, new Money(command.PriceValue, command.PriceIsoCode ?? "EUR"));
            Program.repository.SaveAsync(article);

            return WaitForReadModelUpdate<ArticleReadModelState>();
        }

        [HttpPost]
        [Route("DeactivateArticle")]
        public async Task DeactivateArticle(DeactivateArticle command)
        {
            var article = await Program.repository.GetAsync<Article>(command.Id);

            article.Deactivate(command.Reason);

            await Program.repository.SaveAsync(article);

            await WaitForReadModelUpdate<ArticleReadModelState>();
        }

        [HttpPost]
        [Route("ActivateArticle")]
        public async Task ActivateArticle(ActivateArticle command)
        {
            var article = await Program.repository.GetAsync<Article>(command.Id);

            article.Activate(command.Reason);

            await Program.repository.SaveAsync(article);

            await WaitForReadModelUpdate<ArticleReadModelState>();
        }

        private async Task WaitForReadModelUpdate<TReadModelState>()
        {
            var cp = await Program.repository.GetCurrentCheckpointNumberAsync();
            await Program.checkpointPersister.WaitForCheckpointNumberAsync<TReadModelState>(cp);
        }
    }
}
