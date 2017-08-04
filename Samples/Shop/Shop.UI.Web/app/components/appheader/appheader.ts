module shop {
    export class AppHeader {
        title = ko.observable("");
        entries = ko.observableArray([]);

        constructor(title: string, entries: any) {
            this.title(title);

            if (entries == undefined) {
                entries = [];
            }
            else if (ko.isObservable(entries)) {
                var obs: KnockoutObservableArray<any> = <any>entries;
                obs.subscribe(x => {
                    Enumerable.from(ko.unwrap(x)).forEach(y => {
                        if (!y.componentName || y.componentName == "")
                            y.componentName = "appHeader-command";
                    });
                    this.entries(x);
                });
            }

            Enumerable.from(ko.unwrap(entries)).forEach(x => {
                if (!x.componentName || x.componentName == "")
                    x.componentName = "appHeader-command";
            });

            this.entries = ko.observableArray(ko.unwrap(entries));
        }
    }

    ko.components.register("appheader",
        {
            template: {
                element: "appheader"
            },
            viewModel: function (params) {
                return new AppHeader(params.title, params.entries || []);
            }
        });
}
