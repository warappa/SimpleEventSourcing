using Shop.Core.Domain.Shared;
using SimpleEventSourcing.Domain;
using SimpleEventSourcing.Messaging;
using System;
using System.Linq;

namespace Shop.Core.Domain.Articles
{
    public class Article : AggregateRoot<ArticleState, ArticleId>
    {
        public Article() : base(Enumerable.Empty<IEvent>()) { }

        public Article(ArticleId id, Articlenumber articlenumber, string description, Money price)
            : base(new ArticleCreated(id, articlenumber, description, price, DateTime.UtcNow))
        {

        }

        public void AdjustPrice(Money price)
        {
            ArticleBusinessRules.CanAdjustPrice(price).Check(this);

            RaiseEvent(new ArticlePriceAdjusted(Id, price, DateTime.UtcNow));
        }

        public void ChangeArticlenumber(Articlenumber articlenumber)
        {
            ArticleBusinessRules.CanChangeArticlenumber(articlenumber).Check(this);

            RaiseEvent(new ArticleArticlenumberChanged(Id, articlenumber, DateTime.UtcNow));
        }

        public void Deactivate(string reason)
        {
            ArticleBusinessRules.CanDeactivate(reason).Check(this);

            RaiseEvent(new ArticleDeactivated(Id, reason, DateTime.UtcNow));
        }

        public void Activate(string reason)
        {
            ArticleBusinessRules.CanActivate(reason).Check(this);

            RaiseEvent(new ArticleActivated(Id, reason, DateTime.UtcNow));
        }
    }
}
