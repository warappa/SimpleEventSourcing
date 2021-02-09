using Shop.Core.Domain.Articles;
using Shop.Core.Domain.Customers;
using Shop.Core.Domain.Shared;
using Shop.Core.Domain.ShoppingCarts;
using Shop.Core.Domain.ShoppingCarts.ShoppingCartArticles;
using Shop.Core.Reports.ShoppingCarts.Transient;
using Shop.ReadModel.Articles;
using Shop.Reports.Customers.Transient;
using SimpleEventSourcing.WriteModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Shop
{
    public partial class Program
    {
        private static IObserveRawStreamEntries observer;

        private static async Task Main()
        {
            await ExecuteAsync();
        }

        public static async Task InitializeAsync()
        {
            await SetupWriteModelAsync().ConfigureAwait(false);
            SetupReadModel();
        }

        private static async Task ExecuteAsync()
        {
            await InitializeAsync().ConfigureAwait(false);

            await AnalyseAlmostOrderedAsync();

            await OrderAsync().ConfigureAwait(false);

            await AnalyseAlmostOrderedWithState().ToListAsync();

            CustomersRenameHistory();
            await CustomersRenameHistoryOfGreatCustomerEventStreamAsync().ConfigureAwait(false);
            await CustomersRenameHistoryOfGreatCustomerEventStreamWithEventTypeConstraintsAsync().ConfigureAwait(false);

            AlmostOrderedArticlesReport();

            Console.ReadKey();

            Cleanup();
        }

        public static void Cleanup()
        {
            foreach (var disposeable in disposeables)
            {
                disposeable?.Dispose();
            }
        }

        public static async Task OrderAsync()
        {
            Customer greatCustomer = null;
            Article article = null;
            Article article2 = null;

            greatCustomer = await GetOrCreateGreatCustomerAsync().ConfigureAwait(false);

            article = await GetOrCreateAggregateAsync<ArticleViewModel, Article>(readRepository, x => x.Articlenumber == "A12345", () => new Article(ArticleId.Generate(), "A12345", "A great article", Money.EUR(99.99m)))
                .ConfigureAwait(false);

            article2 = await GetOrCreateAggregateAsync<ArticleViewModel, Article>(readRepository, x => x.Articlenumber == "B12345", () => new Article(ArticleId.Generate(), "B12345", "Another great article", Money.EUR(59.99m)))
                .ConfigureAwait(false);

            var shoppingCart = new ShoppingCart(ShoppingCartId.Generate(), greatCustomer.Id, greatCustomer.StateModel.Name);
            var shoppingCartArticle1 = await shoppingCart.PlaceArticleAsync(ShoppingCartArticleId.Generate(shoppingCart.Id), article.Id, 1, repository);
            await shoppingCart.PlaceArticleAsync(ShoppingCartArticleId.Generate(shoppingCart.Id), article2.Id, 1, repository);
            shoppingCartArticle1.RemoveFromShoppingCart();

            shoppingCart.Order(repository);

            await repository.SaveAsync(shoppingCart);

            var projection = CustomerState.LoadState(greatCustomer.StateModel);
        }

        public static async Task AnalyseAlmostOrderedAsync()
        {
            var observerFactory = new PollingObserverFactory(engine, TimeSpan.FromMilliseconds(500));
            observer = await observerFactory.CreateObserverAsync();

            var removedObservable = observer
                .Where(x => x.PayloadType.StartsWith(nameof(ShoppingCartArticleRemoved)))
                .Select(x => (ShoppingCartArticleRemoved)serializer.Deserialize(x.Payload))
                ;

            var shoppingCartCreatedObservable = observer
                .Where(x => x.PayloadType.StartsWith(nameof(ShoppingCartCreated)))
                .Select(x => (ShoppingCartCreated)serializer.Deserialize(x.Payload));

            var shoppingCartOrderedObservable = observer
                .Where(x => x.PayloadType.StartsWith(nameof(ShoppingCartOrdered)))
                .Select(x => (ShoppingCartOrdered)serializer.Deserialize(x.Payload));

            var orderedWithCustomerNameObservable = shoppingCartOrderedObservable
                .Join(
                    shoppingCartCreatedObservable,
                    _ => Observable.Empty<Unit>(), // triggers window-data reset left side (instant)
                    _ => shoppingCartOrderedObservable, // triggers window-data reset for right side (every change)
                    (orderedAt, created) =>
                    {
                        return new
                        {
                            created.CustomerName,
                            ShoppingCartId = created.Id,
                            ShoppingCartOrdered = orderedAt
                        };
                    }
                )
                .Where(x => x != null);

            await orderedWithCustomerNameObservable
                .Join(
                    removedObservable,
                    _ => Observable.Never<Unit>(), // triggers window-data reset for left side (never)
                    _ => Observable.Never<Unit>(), // triggers window-data reset for right side (never)
                    (ordered, removed) => new { ordered, removed }
                )
                .Where(x => x.ordered.ShoppingCartId.Equals(x.removed.AggregateRootId))
                .Where(x => x.removed.DateTime > x.ordered.ShoppingCartOrdered.DateTime.AddMinutes(-5))
                .Select(x =>
                {
                    return new
                    {
                        x.ordered.ShoppingCartId,
                        x.ordered.CustomerName,
                        relevant = x.removed
                    };
                })
                .Where(x => x != null)
                //.Subscribe(x => Console.WriteLine($"ShoppingCart: {x.CustomerName}: " + string.Join(", ", x.relevant.Articlenumber)))
                ;

            await observer.StartAsync();
        }

        public static async IAsyncEnumerable<AlmostOrderedArticlesState.ShoppingCartArticleRemovedInfo> AnalyseAlmostOrderedWithState()
        {
            var loadedMessages = await engine.LoadStreamEntriesAsync(
                0,
                int.MaxValue, new[]
                {
                    typeof(ShoppingCartCreated),
                    typeof(ShoppingCartArticleRemoved),
                    typeof(ShoppingCartOrdered),
                    typeof(ShoppingCartCancelled)
                })
                .Select(x => serializer.Deserialize(x.Payload))
                .ToListAsync();

            Console.WriteLine("Removed Articles: ");
            var almostOrderedState = AlmostOrderedArticlesState.LoadState((AlmostOrderedArticlesState)null,
                    loadedMessages);

            var removedArticles = almostOrderedState
                    .AlmostOrderedShoppingCartArticles
                    .ToList();

            var groupedByCustomer =
                removedArticles
                    .GroupBy(x => x.CustomerName)
                    .Select(x => new
                    {
                        CustomerName = x.Key,
                        Articlenumbers = x.GroupBy(y => y.Articlenumber)
                            .Select(y => new
                            {
                                Count = y.Count(),
                                Articlenumber = y.Key
                            })
                            .ToList()
                    })
                    .ToList();

            foreach (var item in groupedByCustomer)
            {
                Console.WriteLine($"{item.CustomerName}: {string.Join(", ", item.Articlenumbers.Select(x => "" + x.Count + "x " + x.Articlenumber))} ");
            }
            Console.WriteLine();

            foreach (var item in removedArticles)
            {
                yield return item;
            }
        }

        public static void CustomersRenameHistory()
        {
            var state = new CustomerRenameHistory();
            state = state.Apply(new CustomerCreated("Great-Customer-Id", "Great Customer", DateTime.UtcNow));
            state = state.Apply(new CustomerRenamed("Great-Customer-Id", "Great Customer GmbH", DateTime.UtcNow));
            state = state.Apply(new CustomerRenamed("Great-Customer-Id", "Great Customer AG", DateTime.UtcNow));

            Console.WriteLine("Customers rename history:\n" + state.RenameHistory);
            Console.WriteLine();
        }

        public static async Task CustomersRenameHistoryOfGreatCustomerEventStreamAsync()
        {
            var greatCustomer = await GetOrCreateGreatCustomerAsync().ConfigureAwait(false);

            var events = await engine.LoadStreamEntriesByStreamAsync(greatCustomer.Id)
                .Select(x => serializer.Deserialize(x.Payload))
                .ToListAsync();

            var state = CustomerRenameHistory.LoadState((CustomerRenameHistory)null, events);

            Console.WriteLine("Customers rename history of Great Customer event stream:\n" + state.RenameHistory);
            Console.WriteLine();
        }

        public static async Task CustomersRenameHistoryOfGreatCustomerEventStreamWithEventTypeConstraintsAsync()
        {
            var greatCustomer = await GetOrCreateGreatCustomerAsync().ConfigureAwait(false);

            var events = await engine.LoadStreamEntriesByStreamAsync(
                    greatCustomer.Id,
                    0,
                    int.MaxValue,
                    new[] {
                        typeof(CustomerCreated),
                        typeof(CustomerRenamed)
                    })
                .Select(x => serializer.Deserialize(x.Payload))
                .ToListAsync();

            var state = CustomerRenameHistory.LoadState((CustomerRenameHistory)null, events);

            Console.WriteLine("Customers rename history of Great Customer event stream (with payload type constraints):\n" + state.RenameHistory);
            Console.WriteLine();
        }

        public static void AlmostOrderedArticlesReport()
        {
            var shoppingCartCreated = DateTime.UtcNow;
            var article1Ordered = shoppingCartCreated.AddMinutes(1);
            var article2Ordered = shoppingCartCreated.AddMinutes(2);
            var article3Ordered = shoppingCartCreated.AddMinutes(3);
            var article2Removed = shoppingCartCreated.AddMinutes(4);
            var article1Removed = shoppingCartCreated.AddMinutes(9);
            var ordered = shoppingCartCreated.AddMinutes(10);

            var state = new AlmostOrderedArticlesState();
            state = state.Apply(new ShoppingCartCreated("ShoppingCart-Id", "Great-Customer-Id", "Great Customer", shoppingCartCreated));
            state = state.Apply(new ShoppingCartArticlePlaced("ShoppingCart-Id", "ShoppingCartArticle-Id", "Article-Id", "A12345", "A great Article", Money.EUR(99.99m), 1, Money.EUR(99.99m), article1Ordered));
            state = state.Apply(new ShoppingCartArticlePlaced("ShoppingCart-Id", "ShoppingCartArticle2-Id", "Article2-Id", "B12345", "Another great Article", Money.EUR(59.99m), 1, Money.EUR(99.99m), article2Ordered));
            state = state.Apply(new ShoppingCartArticlePlaced("ShoppingCart-Id", "ShoppingCartArticle3-Id", "Article3-Id", "C12345", "Yet another great Article", Money.EUR(39.99m), 1, Money.EUR(99.99m), article3Ordered));
            state = state.Apply(new ShoppingCartArticleRemoved("ShoppingCart-Id", "ShoppingCartArticle2-Id", "Article2-Id", "B12345", "Yet yet another great Article", Money.EUR(59.99m), 1, Money.EUR(99.99m), article2Removed));
            state = state.Apply(new ShoppingCartArticleRemoved("ShoppingCart-Id", "ShoppingCartArticle-Id", "Article-Id", "A12345", "Yet yet yet another great Article", Money.EUR(99.99m), 1, Money.EUR(99.99m), article1Removed));
            state = state.Apply(new ShoppingCartOrdered("ShoppingCart-Id", ordered));

            Console.WriteLine("Almost ordered shopping cart articles: ");
            foreach (var entry in state.AlmostOrderedShoppingCartArticles)
            {
                Console.WriteLine($"{entry.CustomerName}: {string.Join(", ", entry.Articlenumber)} - {entry.Timespan} before placing order");
            }
            Console.WriteLine();
        }
    }
}
