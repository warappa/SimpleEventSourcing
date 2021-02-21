module shop {
    export interface IShoppingCartArticleRemovedInfo {
        articlenumber: string;
        customerName: string;
        shoppingCartId: string;
        removedAt: Date;
        timespan: number;
        customerId: string;
    }

    export class Customer {
        id: string;
        name: KnockoutObservable<string>;
        title = ko.observable("Customer");

        almostOrderedArticles = ko.observableArray<IShoppingCartArticleRemovedInfo>();

        constructor(id) {
            this.id = id;
            this.name = ko.observable("");

            this.load();
        }

        load() {
            api.customers.getCustomer(this.id).done(x => {
                this.name(x.name);
                this.title('Customer <strong>' + this.name() + '</strong>');
            });

            api.shoppingCarts.getAlmostOrderedArticles(this.id, 0, 0).done(x => {
                this.almostOrderedArticles(x);
            });
        }
    }

    ko.components.register("customer",
        {
            template: {
                element: "customer"
            },
            viewModel: function (params) {
                return new Customer(params.id);
            }
        });
}
