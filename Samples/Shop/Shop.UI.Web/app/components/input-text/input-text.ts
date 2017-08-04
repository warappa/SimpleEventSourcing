module shop {
    export class InputText {
        value = ko.observable("");
        placeholder = ko.observable("");
        label = ko.observable("");

        constructor(value, placeholder, label) {
            this.value = value;
            this.placeholder = placeholder;
            this.label = label;

            this.clear = this.clear.bind(this);
        }

        clear($element) {
            this.value("");

            $($element).prev().focus();
        }
    }

    ko.components.register("input-text",
        {
            template: {
                element: "input-text"
            },
            viewModel: function (params) {
                return new InputText(params.value, params.placeholder, params.label);
            }
        });
}
