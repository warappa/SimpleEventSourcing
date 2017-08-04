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
        public IEnumerable<CustomerViewModel> Get()
        {
            return Program.readRepository.QueryAsync<CustomerViewModel>(x => x.Active).Result.ToList();
        }

        // GET api/<controller>/5
        public CustomerViewModel Get(string id)
        {
            return Program.readRepository.GetByStreamnameAsync<CustomerViewModel>(id).Result;
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
                wkAgg = Program.repository.Get<ShoppingCart>(wk.Streamname);
            }

            wkAgg.PlaceArticle(ShoppingCartArticleId.Generate(wkAgg.Id), command.ArticleId, command.Quantity, Program.repository);

            Program.repository.Save(wkAgg);
        }

        [HttpPost]
        [Route("CreateCustomer")]
        public Task CreateCustomer(CreateCustomer command)
        {
            var customer = new Customer(command.CustomerId, command.Name);
            Program.repository.Save(customer);

            var cpn = Program.repository.GetCurrentCheckpointNumber();
            return Program.checkpointPersister.WaitForCheckpointNumberAsync<CustomerReadModelState>(cpn);
        }
    }
}
