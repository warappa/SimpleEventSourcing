using Shop.Core.Domain.Customers;
using Shop.Core.Domain.ShoppingCarts;
using Shop.Core.Domain.ShoppingCarts.ShoppingCartArticles;
using Shop.ReadModel.Customers;
using Shop.ReadModel.ShoppingCarts;
using Shop.Web.UI.Commands.Customers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace Shop.Web.UI.Controllers
{
    [RoutePrefix("api/customers")]
    public class CustomersController : ApiController
    {
        // GET api/<controller>
        //[Route("get")]
        public async Task<IEnumerable<CustomerViewModel>> Get()
        {
            return await Program.readRepository.QueryAsync<CustomerViewModel>(x => x.Active);
        }

        // GET api/<controller>/5
        public async Task<CustomerViewModel> Get(string id)
        {
            return await Program.readRepository.GetByStreamnameAsync<CustomerViewModel>(id);
        }

        [HttpPost]
        [Route("OrderArticle")]
        public async Task OrderArticle(OrderArticle command)
        {
            string wkId = null;
            ShoppingCart wkAgg = null;
            var wk = (await Program.readRepository.QueryAsync<ShoppingCartViewModel>(x => x.CustomersId == command.CustomerId && x.Status == (int)ShoppingCartStatus.Open))
                .FirstOrDefault();

            if (wk == null)
            {
                var customer = (await Program.readRepository.QueryAsync<CustomerViewModel>(x => x.Streamname == command.CustomerId))
                    .FirstOrDefault();

                wkId = Guid.NewGuid().ToString();
                wkAgg = new ShoppingCart(wkId, command.CustomerId, customer.Name);
            }
            else
            {
                wkAgg = await Program.repository.GetAsync<ShoppingCart>(wk.Streamname);
            }

            await wkAgg.PlaceArticleAsync(ShoppingCartArticleId.Generate(wkAgg.Id), command.ArticleId, command.Quantity, Program.repository);

            await Program.repository.SaveAsync(wkAgg);
        }

        [HttpPost]
        [Route("CreateCustomer")]
        public async Task CreateCustomer(CreateCustomer command)
        {
            var customer = new Customer(command.CustomerId, command.Name);
            var checkpointNumber = await Program.repository.SaveAsync(customer);

            await Program.checkpointPersister.WaitForCheckpointNumberAsync<CustomerReadModelState>(checkpointNumber);
        }
    }
}
