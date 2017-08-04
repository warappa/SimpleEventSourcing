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
        public IEnumerable<ArticleViewModel> Get()
        {
            return Program.readRepository.QueryAsync<ArticleViewModel>(x => true).Result.ToList();
        }

        public ArticleViewModel Get(string id)
        {
            return Program.readRepository.GetByStreamnameAsync<ArticleViewModel>(id).Result;
        }

        [HttpGet]
        [Route("GetArticleActivationHistory")]
        public IEnumerable<ArticleActivationHistoryViewModel> GetArticleActivationHistory(string id)
        {
            return Program.readRepository.QueryAsync<ArticleActivationHistoryViewModel>(x => x.Streamname == id).Result;
        }

        [HttpPost]
        [Route("CreateArticle")]
        public Task CreateArticle(CreateArticle command)
        {
            var article = new Article(command.Id, command.Articlenumber, command.Description, new Money(command.PriceValue, command.PriceIsoCode ?? "EUR"));
            Program.repository.Save(article);

            return WaitForReadModelUpdate<ArticleReadModelState>();
        }

        [HttpPost]
        [Route("DeactivateArticle")]
        public Task DeactivateArticle(DeactivateArticle command)
        {
            var article = Program.repository.Get<Article>(command.Id);

            article.Deactivate(command.Reason);

            Program.repository.Save(article);

            return WaitForReadModelUpdate<ArticleReadModelState>();
        }

        [HttpPost]
        [Route("ActivateArticle")]
        public Task ActivateArticle(ActivateArticle command)
        {
            var article = Program.repository.Get<Article>(command.Id);

            article.Activate(command.Reason);

            Program.repository.Save(article);

            return WaitForReadModelUpdate<ArticleReadModelState>();
        }

        private Task WaitForReadModelUpdate<TReadModelState>()
        {
            var cp = Program.repository.GetCurrentCheckpointNumber();
            return Program.checkpointPersister.WaitForCheckpointNumberAsync<TReadModelState>(cp);
        }
    }
}
