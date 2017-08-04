module shop {
    export class Login {
        username = ko.observable("");
        password = ko.observable("");

        constructor(username: string) {
            this.username(username || "");
            this.$commit = this.$commit.bind(this);
        }

        $commit() {
            location.hash = "article-overview";
        }
    }

    ko.components.register("login",
        {
            template: {
                element: "login"
            },
            viewModel: function (params) {
                if (params === undefined) {
                    return new Login("");
                }

                return new Login(params.username);
            }
        });
}
