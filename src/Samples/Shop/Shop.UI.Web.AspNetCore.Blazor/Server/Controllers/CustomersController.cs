using Microsoft.AspNetCore.Mvc;
using Shop.Core.Domain.Customers;
using Shop.Core.Domain.ShoppingCarts;
using Shop.Core.Domain.ShoppingCarts.ShoppingCartArticles;
using Shop.ReadModel.Customers;
using Shop.ReadModel.ShoppingCarts;
using Shop.UI.Web.Shared.Commands.Customers;
using SimpleEventSourcing.ReadModel;
using SimpleEventSourcing.WriteModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shop.UI.Web.AspNetCore.Blazor.Server.Controllers
{
    [ApiController]
    [Route("api/customers")]
    public class CustomersController : ControllerBase
    {
        private readonly IReadRepository readRepository;
        private readonly IEventRepository repository;
        private readonly ICheckpointPersister checkpointPersister;

        public CustomersController(IReadRepository readRepository, IEventRepository repository,
            ICheckpointPersister checkpointPersister)
        {
            this.readRepository = readRepository ?? throw new ArgumentNullException(nameof(readRepository));
            this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
            this.checkpointPersister = checkpointPersister ?? throw new ArgumentNullException(nameof(checkpointPersister));
        }

        [HttpGet]
        public async Task<IEnumerable<CustomerViewModel>> Get()
        {
            return await readRepository.QueryAsync<CustomerViewModel>(x => x.Active);
        }

        [HttpGet("{id}")]
        public async Task<CustomerViewModel> Get(string id)
        {
            return await readRepository.GetByStreamnameAsync<CustomerViewModel>(id);
        }

        [HttpPost]
        [Route("OrderArticle")]
        public async Task OrderArticle(OrderArticle command)
        {
            string wkId = null;
            ShoppingCart wkAgg = null;
            var wk = (await readRepository.QueryAsync<ShoppingCartViewModel>(x => x.CustomersId == command.CustomerId && x.Status == (int)ShoppingCartStatus.Open))
                .FirstOrDefault();

            if (wk == null)
            {
                var customer = (await readRepository.QueryAsync<CustomerViewModel>(x => x.Streamname == command.CustomerId))
                    .FirstOrDefault();

                wkId = Guid.NewGuid().ToString();
                wkAgg = new ShoppingCart(wkId, command.CustomerId, customer.Name);
            }
            else
            {
                wkAgg = await repository.GetAsync<ShoppingCart>(wk.Streamname);
            }

            await wkAgg.PlaceArticleAsync(ShoppingCartArticleId.Generate(wkAgg.Id), command.ArticleId, command.Quantity, repository);

            await repository.SaveAsync(wkAgg);
        }

        [HttpPost]
        [Route("CreateCustomer")]
        public async Task CreateCustomer(CreateCustomer command)
        {
            var customer = new Customer(command.CustomerId, command.Name);
            var checkpointNumber = await repository.SaveAsync(customer);

            await checkpointPersister.WaitForCheckpointNumberAsync<CustomerReadModelState>(checkpointNumber);
        }
    }
}
