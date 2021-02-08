module shop {
    export class Application {
        text = ko.observable("");
        currentPage = ko.observable<IPageInfo>();
        router = crossroads.create();

        constructor(text: string) {
            var self = this;
            this.text = ko.observable(text);

            this.router.addRoute("{page}{?query}", this.setPage.bind(this));
            this.router.addRoute("{page}", this.setPage.bind(this));
            this.router.addRoute("", () => this.setPage("article-overview", {}));

            this.router.parse("login?username=abc");

            function parseHash(newHash, oldHash) {
                self.router.parse(newHash);
            }
            hasher.initialized.add(parseHash);
            hasher.changed.add(parseHash);
            hasher.init();

        }

        setPage(page, query) {
            this.currentPage({ page: page, query: query });
        }
    }

    export interface IPageInfo {
        page: string;
        query: any;
    }

    ko.components.register("application",
        {
            template: {
                element: "application"
            },
            viewModel: function (params) {
                return new Application(params.text);
            }
        });
}
