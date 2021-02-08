module shop {
    export class InputPassword {
        value = ko.observable("");
        placeholder = ko.observable("");

        constructor(value, placeholder) {
            this.value = value;
            this.placeholder = placeholder;

            this.clear = this.clear.bind(this);
        }

        clear($element) {
            this.value("");

            $($element).prev().focus();
        }
    }

    ko.components.register("input-password",
        {
            template: {
                element: "input-password"
            },
            viewModel: function (params) {
                return new InputPassword(params.value, params.placeholder);
            }
        });
}
