module shop {
    export interface IAggregateDto {
        streamname: string;
    }

    export interface IArticleDto extends IAggregateDto {
        articlenumber: string;
        active: boolean;
    }

    export interface IArticleActivationHistoryDto extends IAggregateDto {
        active: boolean;
        date: Date;
        articlenumber: string;
        reason: string;
    }

    export interface IShoppingCartDto extends IAggregateDto {
        customerId: string;
        customerName: string;
    }

    export interface IShoppingCartArticleDto extends IAggregateDto {
        shoppingCartArticleId: string;
        articlenumber: string;
        description: string;
        createdAt: Date;
        priceValue: number;
        priceIsoCode: string;
    }

    export interface IAlmostOrderedDto extends IAggregateDto {
        articlenumber: string;
        description: string;
        createdAt: Date;
        priceValue: number;
        priceIsoCode: string;
    }

    export interface ICustomerDto extends IAggregateDto {
        name: string;
    }

    export module api {
        export function postJSON(url: string, data: any, handler: (data: any, textStatus: string, jqXHR: JQueryXHR) => any): JQueryPromise<any> {
            return $.post(url, data, handler, 'json')
                .fail((jq: JQueryXHR) => {
                    var error = jq.responseJSON;
                    alert(error.exceptionMessage);
                });
        }

        export function getJSON(url: string, data: any, success: (data: any, textStatus: string, jqXHR: JQueryXHR) => any): JQueryPromise<any> {
            return $.getJSON.apply(this, arguments)
                .fail((jq: JQueryXHR) => {
                    var error = jq.responseJSON;
                    alert(error.exceptionMessage);
                });
        }

        export module articles {
            export function getArticleList(page, pageSize): JQueryDeferred<IArticleDto[]> {
                var deferred = $.Deferred<IArticleDto[]>();
                api.getJSON("api/articles", null, data => deferred.resolve(data));
                return deferred;
            }

            export function getArticle(id: string): JQueryDeferred<IArticleDto> {
                var deferred = $.Deferred<IArticleDto>();
                api.getJSON("api/articles", { id: id }, data => deferred.resolve(data));
                return deferred;
            }

            export function getArticleActivationHistory(id: string): JQueryDeferred<IArticleActivationHistoryDto[]> {
                var deferred = $.Deferred<IArticleActivationHistoryDto[]>();
                api.getJSON("api/articles/getArticleActivationHistory", { id: id }, data => deferred.resolve(data));
                return deferred;
            }

            export function createArticle(id: string, articlenumber: string, description: string, priceValue: number, priceIsoCode: string): JQueryDeferred<any> {
                var deferred = $.Deferred();
                api.postJSON("api/articles/createArticle", { id: id, articlenumber: articlenumber, description: description, priceValue: priceValue, priceIsoCode: priceIsoCode }, data => deferred.resolve(data));
                return deferred;
            }

            export function deactivateArticle(id: string, reason: string): JQueryDeferred<any> {
                var deferred = $.Deferred();
                api.postJSON("api/articles/deactivateArticle", { id: id, reason: reason }, data => deferred.resolve(data));
                return deferred;
            }

            export function activateArticle(id: string, reason: string): JQueryDeferred<any> {
                var deferred = $.Deferred();
                api.postJSON("api/articles/activateArticle", { id: id, reason: reason }, data => deferred.resolve(data));
                return deferred;
            }
        }

        export module shoppingCarts {
            export function getShoppingCarts(page, pageSize): JQueryDeferred<IShoppingCartDto[]> {
                var deferred = $.Deferred<IShoppingCartDto[]>();
                api.getJSON("api/shoppingcarts", null, data => deferred.resolve(data));
                return deferred;
            }

            export function getShoppingCart(id: string): JQueryDeferred<IShoppingCartDto> {
                var deferred = $.Deferred<IShoppingCartDto>();
                api.getJSON("api/shoppingcarts", { id: id }, data => deferred.resolve(data));
                return deferred;
            }

            export function getShoppingCartsForCustomer(customerId, page, pageSize): JQueryDeferred<IShoppingCartDto[]> {
                var deferred = $.Deferred<IShoppingCartDto[]>();
                api.getJSON("api/shoppingcarts", { customerId: customerId }, data => deferred.resolve(data));
                return deferred;
            }

            export function getAlmostOrderedArticles(customerId, page, pageSize): JQueryDeferred<IShoppingCartArticleEntferntInfo[]> {
                var deferred = $.Deferred<IShoppingCartArticleEntferntInfo[]>();
                api.getJSON("api/shoppingcarts/getAlmostOrderedArticles", { customerId: customerId }, data => deferred.resolve(data));
                return deferred;
            }

            export function removeArticleFromShoppingCart(shoppingCartId: string, shoppingCartArticleId: string): JQueryDeferred<ICustomerDto[]> {
                var deferred = $.Deferred<ICustomerDto[]>();
                api.postJSON("api/shoppingcarts/removeArticleFromShoppingCart", { shoppingCartId: shoppingCartId, shoppingCartArticleId: shoppingCartArticleId }, data => deferred.resolve(data));
                return deferred;
            }

            export function order(shoppingCartId: string): JQueryDeferred<ICustomerDto[]> {
                var deferred = $.Deferred<ICustomerDto[]>();
                api.postJSON("api/shoppingcarts/order", { shoppingCartId: shoppingCartId }, data => deferred.resolve(data));
                return deferred;
            }
        }

        export module customers {
            export function getCustomers(page, pageSize): JQueryDeferred<ICustomerDto[]> {
                var deferred = $.Deferred<ICustomerDto[]>();
                api.getJSON("api/customers", null, data => deferred.resolve(data));
                return deferred;
            }

            export function getCustomer(id: string): JQueryDeferred<ICustomerDto> {
                var deferred = $.Deferred<ICustomerDto>();
                api.getJSON("api/customers", { id: id }, data => deferred.resolve(data));
                return deferred;
            }

            export function orderArticle(customerId: string, articleId: string, quantity: number): JQueryDeferred<ICustomerDto[]> {
                var deferred = $.Deferred<ICustomerDto[]>();
                api.postJSON("api/customers/orderArticle", { customerId: customerId, articleId: articleId, quantity: quantity }, data => deferred.resolve(data));
                return deferred;
            }

            export function createCustomer(customerId: string, name: string): JQueryDeferred<any> {
                var deferred = $.Deferred<any>();
                api.postJSON("api/customers/createCustomer", { customerId: customerId, name: name }, data => deferred.resolve(data));
                return deferred;
            }
        }
    }
}
