module shop {
    export class Article {
        id: string;
        name: KnockoutObservable<string>;
        title = ko.observable("Article");
        model = ko.observable<IArticleDto>();

        menuItems = ko.observableArray([]);
        history = ko.observableArray([]);

        constructor(id) {
            this.getOrderData = this.getOrderData.bind(this);
            this.getActivateData = this.getActivateData.bind(this);
            this.getDeactivateData = this.getDeactivateData.bind(this);

            this.id = id;
            this.name = ko.observable("");
            this.model = ko.observable<IArticleDto>();

            this.menuItems = ko.observableArray([]);
            this.history = ko.observableArray([]);

            this.load();
        }

        load() {
            api.articles.getArticle(this.id).done(x => {
                this.model(x);
                this.name(x.articlenumber);
                this.title('Article <strong>' + this.name() + '</strong>');

                this.menuItems.removeAll();
                if (x.active) {
                    this.menuItems.push({ template: 'order', title: 'Order' });
                    this.menuItems.push({ template: 'article-deactivate', title: 'Deactivate' });
                }
                else {
                    this.menuItems.push({ template: 'article-activate', title: 'Activate' });
                }
            });

            api.articles.getArticleActivationHistory(this.id).done(x => this.history(x.sort((a, b) => a.date > b.date ? -1 : 1)));
        }

        getOrderData($context) {
            var obj = {
                id: this.id,
                customerId: ko.observable(""),
                customers: ko.observableArray([]),
                quantity: ko.observable(1),

                $commit: (data) => {
                    api.customers.orderArticle(data.customerId(), data.id, Number(data.quantity()))
                        .done(() => {
                            obj.$visible(false);
                        });
                },
                $visible: ko.observable(true)
            };

            api.customers.getCustomers(0, 0).done(x => obj.customers(x));

            return obj;
        }

        getDeactivateData($context) {
            var obj = {
                id: this.id,
                reason: ko.observable(""),

                $commit: (data) => {
                    api.articles.deactivateArticle(data.id, data.reason())
                        .done(() => {
                            obj.$visible(false);
                            this.load();
                        });
                },
                $visible: ko.observable(true)
            };

            return obj;
        }

        getActivateData($context) {
            var obj = {
                id: this.id,
                reason: ko.observable(""),

                $commit: (data) => {
                    api.articles.activateArticle(data.id, data.reason())
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

    ko.components.register("article",
        {
            template: {
                element: "article"
            },
            viewModel: function (params) {
                return new Article(params.id);
            }
        });
}
