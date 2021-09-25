using Microsoft.AspNetCore.Mvc;
using Shop.Core.Domain.Articles;
using Shop.Core.Domain.Shared;
using Shop.ReadModel.Articles;
using Shop.UI.Web.Shared.Commands.Articles;
using SimpleEventSourcing.ReadModel;
using SimpleEventSourcing.State;
using SimpleEventSourcing.WriteModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shop.UI.Web.AspNetCore.Blazor.Server.Controllers
{
    [ApiController]
    [Route("api/articles")]
    public partial class ArticlesController : ControllerBase
    {
        private readonly IReadRepository readRepository;
        private readonly IEventRepository repository;
        private readonly ICheckpointPersister checkpointPersister;

        public ArticlesController(IReadRepository readRepository, IEventRepository repository,
            ICheckpointPersister checkpointPersister)
        {
            this.readRepository = readRepository ?? throw new ArgumentNullException(nameof(readRepository));
            this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
            this.checkpointPersister = checkpointPersister ?? throw new ArgumentNullException(nameof(checkpointPersister));
        }

        [HttpGet]
        public async Task<IEnumerable<ArticleViewModel>> Get()
        {
            return (await readRepository.QueryAsync<ArticleViewModel>(x => true)).ToList();
        }

        [HttpGet("{id}")]
        public async Task<ArticleViewModel> Get(string id)
        {
            return await readRepository.GetByStreamnameAsync<ArticleViewModel>(id);
        }

        [HttpGet]
        [Route("GetArticleActivationHistory")]
        public async Task<IEnumerable<ArticleActivationHistoryViewModel>> GetArticleActivationHistory(string id)
        {
            return await readRepository.QueryAsync<ArticleActivationHistoryViewModel>(x => x.Streamname == id);
        }

        [HttpPost]
        [Route("CreateArticle")]
        public async Task CreateArticle(CreateArticle command)
        {
            var article = new Article(command.Id, command.Articlenumber, command.Description, new Money(command.PriceValue, command.PriceIsoCode ?? "EUR"));
            await repository.SaveAsync(article);

            await WaitForReadModelUpdate<ArticleReadModelState>();
        }

        [HttpPost]
        [Route("DeactivateArticle")]
        public async Task DeactivateArticle(DeactivateArticle command)
        {
            var article = await repository.GetAsync<Article>(command.Id);

            article.Deactivate(command.Reason);

            await repository.SaveAsync(article);

            await WaitForReadModelUpdate<ArticleReadModelState>();
        }

        [HttpPost]
        [Route("ActivateArticle")]
        public async Task ActivateArticle(ActivateArticle command)
        {
            var article = await repository.GetAsync<Article>(command.Id);

            article.Activate(command.Reason);

            await repository.SaveAsync(article);

            await WaitForReadModelUpdate<ArticleReadModelState>();
        }

        private async Task WaitForReadModelUpdate<TReadModelState>()
            where TReadModelState : IAsyncState
        {
            var cp = await repository.GetCurrentCheckpointNumberAsync();
            await checkpointPersister.WaitForCheckpointNumberAsync<TReadModelState>(cp);
        }
    }
}
