module shop {
    export class ArticleOverview {
        searchTerm = ko.observable("");
        articleList = ko.observableArray([]);
        results: KnockoutComputed<IArticleDto[]>;

        constructor() {
            this.getCreateArticleData = this.getCreateArticleData.bind(this);
            this.articleList([]);

            this.results = ko.computed(() => {
                return Enumerable.from(this.articleList())
                    .where(x => x.articlenumber.toLowerCase().indexOf(this.searchTerm().toLowerCase()) >= 0)
                    .toArray();
            });

            this.load();
        }

        load() {
            shop.api.articles.getArticleList(1, 20).done(x => {
                return this.articleList(x);
            });
        }

        open(data: IArticleDto) {
            location.hash = "article?id=" + data.streamname;
        }

        getCreateArticleData($context) {
            var obj = {
                id: "" + Math.random() * 1000,
                articlenumber: ko.observable(""),
                description: ko.observable(""),
                priceValue: ko.observable(0),
                priceIsoCode: ko.observable(""),

                currencies: [
                    "EUR",
                    "USD"
                ],

                $commit: (data) => {
                    api.articles.createArticle(data.id, data.articlenumber(), data.description(), data.priceValue(), data.priceIsoCode())
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

    ko.components.register("article-overview",
        {
            template: {
                element: "article-overview"
            },
            viewModel: function (params) {
                return new ArticleOverview();
            }
        });
}
