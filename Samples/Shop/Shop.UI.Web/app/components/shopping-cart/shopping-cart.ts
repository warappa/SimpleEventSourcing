module shop {
    export class ShoppingCart {
        id: string;
        name: KnockoutObservable<string>;
        title = ko.observable("Shopping Cart");
        model = ko.observable<IShoppingCartDto>();

        constructor(id) {
            this.id = id;
            this.name = ko.observable("");

            this.load();
        }

        load() {
            api.shoppingCarts.getShoppingCart(this.id).done(x => {
                this.model(x);
                this.name(x.customerName);
                this.title('Shopping cart for <strong>' + this.name() + '</strong>');
            });
        }

        removeArticleFromShoppingCart(data: IShoppingCartArticleDto) {
            api.shoppingCarts.removeArticleFromShoppingCart(this.id, data.shoppingCartArticleId)
                .then(x => this.load());
        }

        order() {
            if (confirm("Do you really want to order?")) {
                api.shoppingCarts.order(this.id)
                    .then(x => this.load());
            }
        }
    }

    ko.components.register("shopping-cart",
        {
            template: {
                element: "shopping-cart"
            },
            viewModel: function (params) {
                return new ShoppingCart(params.id);
            }
        });
}
