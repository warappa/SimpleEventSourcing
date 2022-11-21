using Microsoft.AspNetCore.Mvc;
using Shop.Core.Domain.ShoppingCarts;
using Shop.Core.Domain.ShoppingCarts.ShoppingCartArticles;
using Shop.ReadModel.ShoppingCarts;
using Shop.Reports.ShoppingCarts.Transient;
using Shop.UI.Web.Shared.Commands.ShoppingCarts;
using Shop.UI.Web.Shared.ReadModels.ShoppingCarts;
using SimpleEventSourcing.ReadModel;
using SimpleEventSourcing.WriteModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shop.UI.Web.AspNetCore.Blazor.Server.Controllers
{
    [ApiController]
    [Route("api/shoppingcarts")]
    public class ShoppingCartsController : ControllerBase
    {
        private readonly IReadRepository readRepository;
        private readonly IEventRepository repository;
        private readonly ICheckpointPersister checkpointPersister;
        private readonly IPersistenceEngine engine;

        public ShoppingCartsController(IReadRepository readRepository, IEventRepository repository,
            ICheckpointPersister checkpointPersister,
            IPersistenceEngine engine)
        {
            this.readRepository = readRepository ?? throw new ArgumentNullException(nameof(readRepository));
            this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
            this.checkpointPersister = checkpointPersister ?? throw new ArgumentNullException(nameof(checkpointPersister));
            this.engine = engine;
        }

        [HttpGet]
        public async Task<IEnumerable<ShoppingCartViewModel>> Get(string customerId = null, int? page = null, int? pageSize = null)
        {
            return (await readRepository.QueryAsync<ShoppingCartViewModel>(x => x.Active && (customerId == null || x.CustomersId == customerId)))
                .OrderByDescending(x => x.CreatedAt)
                .ToList();
        }

        [HttpGet("{id}")]
        public async Task<ShoppingCartViewModelExt> Get(string id)
        {
            var wk = await readRepository.GetByStreamnameAsync<ShoppingCartViewModel>(id);

            var wkId = wk.ShoppingCartId;

            var wkExt = new ShoppingCartViewModelExt
            {
                ShoppingCartId = wk.ShoppingCartId,
                Id = wk.Id,
                CustomersId = wk.CustomersId,
                CustomerName = wk.CustomerName,
                Status = wk.Status,
                ShoppingCartArticles = (await readRepository
                    .QueryAsync<ShoppingCartArticleViewModel>(x => x.Streamname == wkId))
                    .ToList()
            };

            return wkExt;
        }

        [HttpGet]
        [Route("GetAlmostOrderedArticles")]
        public async Task<IEnumerable<AlmostOrderedArticlesState.ShoppingCartArticleRemovedInfo>> GetAlmostOrderedArticles(string customerId)
        {
            return await Shop.Program.AnalyseAlmostOrderedWithState(engine)
                .Where(x => x.CustomerId == customerId)
                .ToListAsync();
        }

        [HttpPost]
        [Route("RemoveArticleFromShoppingCart")]
        public async Task RemoveArticleFromShoppingCart(RemoveArticleFromShoppingCart command)
        {
            var wk = await repository.GetAsync<ShoppingCart>(command.ShoppingCartId);
            var wka = wk.GetChildEntity<ShoppingCartArticle>(command.ShoppingCartArticleId);

            wka.RemoveFromShoppingCart();

            var checkpointNumber = await repository.SaveAsync(wk);

            await checkpointPersister.WaitForCheckpointNumberAsync<ShoppingCartReadModelState>(checkpointNumber);
        }

        [HttpPost]
        [Route("Order")]
        public async Task Order(OrderShoppingCart command)
        {
            var wk = await repository.GetAsync<ShoppingCart>(command.ShoppingCartId);

            wk.Order(repository);

            var checkpointNumber = await repository.SaveAsync(wk);

            await checkpointPersister.WaitForCheckpointNumberAsync<ShoppingCartReadModelState>(checkpointNumber);
        }
    }
}
