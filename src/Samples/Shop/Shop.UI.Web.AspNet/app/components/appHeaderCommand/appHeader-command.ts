module shop {
    export class AppHeaderCommand {
        title = "";
        href = "";
        template = "";
        action: () => any;
        initAction: () => any;

        constructor(title: string, href: string, action: () => any, initAction: () => any, template: string) {
            this.title = title;
            this.href = href;
            this.action = action;
            this.initAction = initAction;
            this.template = template;
        }
    }

    ko.components.register("appHeader-command",
        {
            template: {
                element: "appHeader-command"
            },
            viewModel: function (params) {
                return new AppHeaderCommand(params.title, params.href, params.action, params.initAction, params.template);
            }
        });
}
