module shop {
    export class Tile {
        texts: KnockoutObservableArray<string> = ko.observableArray([]);
        footer: KnockoutObservable<string> = ko.observable("");
        backgroundColor: KnockoutObservable<string> = ko.observable("blue");

        constructor(texts: string[], footer: string, backgroundColor: string) {
            this.texts = ko.observableArray(texts);
            this.footer = ko.observable(footer);
            this.backgroundColor = ko.observable(ko.unwrap(backgroundColor));
        }
    }

    ko.components.register("tile",
        {
            template: {
                element: "tile"
            },
            viewModel: function (params) {
                return new Tile(params.texts, params.footer, params.backgroundColor || "blue");
            }
        });
}
