module shop {
    export class ShoppingCartsOverview {
        searchTerm = ko.observable("");
        shoppingCarts = ko.observableArray([]);

        results: KnockoutComputed<ICustomerDto[]>;

        filter = ko.observable("all");
        filters = [
            { id: "all", text: "All" }
        ];

        constructor() {
            this.shoppingCarts([]);

            this.results = ko.computed(() => {
                return Enumerable.from(this.shoppingCarts())
                    .where(x => x.customerName.toLowerCase().indexOf(this.searchTerm().toLowerCase()) >= 0)
                    .toArray();
            }, this);

            api.shoppingCarts.getShoppingCarts(0, 100).done(x => {
                this.shoppingCarts(x);
            });
        }

        open(data: IShoppingCartDto) {
            location.hash = "shopping-cart?id=" + data.streamname;
        }
    }

    ko.components.register("shopping-carts-overview",
        {
            template: {
                element: "shopping-carts-overview"
            },
            viewModel: function (params) {
                return new ShoppingCartsOverview();
            }
        });
}
