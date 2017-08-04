module shop {
    export class NavigationContainer {
        currentPage: KnockoutObservable<IPageInfo>;

        constructor(currentPage: KnockoutObservable<IPageInfo>) {
            this.currentPage = currentPage;
        }
    }

    ko.components.register("navigationcontainer",
        {
            template: {
                element: "navigationcontainer"
            },
            viewModel: function (params) {
                return new NavigationContainer(params.currentPage);
            }
        });
}
