namespace Shop.UI.Web.Shared.Commands.Articles
{
    public class CreateArticle
    {
        public string Id { get; set; }
        public string Articlenumber { get; set; }
        public string Description { get; set; }
        public decimal PriceValue { get; set; }
        public string PriceIsoCode { get; set; }
    }
}
