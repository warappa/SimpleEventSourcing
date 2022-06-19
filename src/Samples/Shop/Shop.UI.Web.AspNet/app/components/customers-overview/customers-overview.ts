module shop {
    export class CustomersOverview {
        searchTerm = ko.observable("");
        customers = ko.observableArray([]);

        results: KnockoutComputed<ICustomerDto[]>;

        filter = ko.observable("all");
        filters = [
            { id: "all", text: "All" }
        ];

        constructor() {
            this.getCreateCustomerData = this.getCreateCustomerData.bind(this);

            this.customers([]);

            this.results = ko.computed(() => {
                return Enumerable.from(this.customers())
                    .where(x => x.name.toLowerCase().indexOf(this.searchTerm().toLowerCase()) >= 0)
                    .toArray();
            }, this);

            this.load();
        }

        load() {
            api.customers.getCustomers(0, 100).done(x => {
                this.customers(x);
            });
        }

        open(data: ICustomerDto) {
            location.hash = "customer?id=" + data.streamname;
        }

        getCreateCustomerData($context) {
            var obj = {
                id: "" + Math.random() * 1000,
                name: ko.observable(""),

                $commit: (data) => {
                    api.customers.createCustomer(data.id, data.name())
                        .done(() => {
                            obj.$visible(false);
                            this.load();
                        });
                },
                $visible: ko.observable(true)
            };
            return obj;
        }
    }

    ko.components.register("customers-overview",
        {
            template: {
                element: "customers-overview"
            },
            viewModel: function (params) {
                return new CustomersOverview();
            }
        });
}
