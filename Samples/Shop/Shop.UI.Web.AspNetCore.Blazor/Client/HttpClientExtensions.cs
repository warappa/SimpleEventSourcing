using Shop.Core.BusinessRules;
using Shop.ReadModel.Shared;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Shop.UI.Web.AspNetCore.Blazor.Client
{
    public static class HttpClientExtensions
    {
        public static async Task CheckForBusinessRuleExceptionAsync(this HttpResponseMessage message)
        {
            if (!message.IsSuccessStatusCode)
            {
                var exc = await message.Content.ReadFromJsonAsync<ExceptionDto>();
                var errors = exc.Errors
                    .Select(x => new BusinessRuleValidationError(x.Key, x.Message))
                    .ToList();
                throw new BusinessRuleException(exc.Name, errors);
            }
        }
    }
}
