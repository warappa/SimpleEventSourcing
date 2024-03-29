﻿using Shop.Core.Domain.ShoppingCarts;
using Shop.Core.Domain.ShoppingCarts.ShoppingCartArticles;
using Shop.ReadModel.ShoppingCarts;
using Shop.Reports.ShoppingCarts.Transient;
using Shop.UI.Web.AspNet.Commands.ShoppingCarts;
using Shop.UI.Web.Shared.ReadModels.ShoppingCarts;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace Shop.UI.Web.AspNet.Controllers
{
    [RoutePrefix("api/shoppingcarts")]
    public class ShoppingCartsController : ApiController
    {
        // GET api/<controller>
        //[Route("get")]
        public async Task<IEnumerable<ShoppingCartViewModel>> Get(string customerId = null, int? page = null, int? pageSize = null)
        {
            return (await Program.readRepository.QueryAsync<ShoppingCartViewModel>(x => x.Active && (customerId == null || x.CustomersId == customerId)))
                .OrderByDescending(x => x.CreatedAt)
                .ToList();
        }

        public async Task<ShoppingCartViewModelExt> Get(string id)
        {
            var wk = await Program.readRepository.GetByStreamnameAsync<ShoppingCartViewModel>(id);

            var wkId = wk.ShoppingCartId;

            var wkExt = new ShoppingCartViewModelExt
            {
                ShoppingCartId = wk.ShoppingCartId,
                Id = wk.Id,
                CustomersId = wk.CustomersId,
                CustomerName = wk.CustomerName,
                Status = wk.Status,
                ShoppingCartArticles = (await Program.readRepository
                .QueryAsync<ShoppingCartArticleViewModel>(x => x.Streamname == wkId))
                .ToList()
            };

            return wkExt;
        }

        [HttpGet]
        [Route("GetAlmostOrderedArticles")]
        public async Task<IEnumerable<AlmostOrderedArticlesState.ShoppingCartArticleRemovedInfo>> GetAlmostOrderedArticles(string customerId)
        {
            return await Program.AnalyseAlmostOrderedWithState()
                .Where(x => x.CustomerId == customerId)
                .ToListAsync();
        }

        [HttpPost]
        [Route("RemoveArticleFromShoppingCart")]
        public async Task RemoveArticleFromShoppingCart(RemoveArticleFromShoppingCart command)
        {
            var wk = await Program.repository.GetAsync<ShoppingCart>(command.ShoppingCartId);
            var wka = wk.GetChildEntity<ShoppingCartArticle>(command.ShoppingCartArticleId);

            wka.RemoveFromShoppingCart();

            var checkpointNumber = await Program.repository.SaveAsync(wk);

            await Program.checkpointPersister.WaitForCheckpointNumberAsync<ShoppingCartReadModelState>(checkpointNumber);
        }

        [HttpPost]
        [Route("Order")]
        public async Task Order(OrderShoppingCart command)
        {
            var wk = await Program.repository.GetAsync<ShoppingCart>(command.ShoppingCartId);

            wk.Order(Program.repository);

            var checkpointNumber = await Program.repository.SaveAsync(wk);

            await Program.checkpointPersister.WaitForCheckpointNumberAsync<ShoppingCartReadModelState>(checkpointNumber);
        }
    }
}
