using Shop.Core.Domain.Shared;
using SimpleEventSourcing.State;
using SimpleEventSourcing.WriteModel;
using System;

namespace Shop.Core.Domain.Articles
{
    [Versioned("ArticleState", 0)]
    public class ArticleState : AggregateRootState<ArticleState, ArticleId>
    {
        public string Articlenumber { get; private set; }
        public Money Price { get; private set; }
        public bool Active { get; private set; }
        public string Description { get; private set; }

        public ArticleState() { }
        public ArticleState(ArticleState state)
            : base(state)
        {
            Articlenumber = state.Articlenumber;
            Price = state.Price;
            Active = state.Active;
            Description = state.Description;
        }

        public ArticleState Apply(ArticleCreated @event)
        {
            var s = new ArticleState(this);
            s.StreamName = @event.Id;
            s.Articlenumber = @event.Articlenumber;
            s.Description = @event.Description;
            s.Price = @event.Price;
            s.Active = true;

            return s;
        }

        public ArticleState Apply(ArticleDeactivated @event)
        {
            var s = new ArticleState(this);
            s.Active = false;

            return s;
        }

        public ArticleState Apply(ArticleActivated @event)
        {
            var s = new ArticleState(this);
            s.Active = true;

            return s;
        }

        public override object ConvertFromStreamName(Type tkey, string streamName)
        {
            return new ArticleId(streamName);
        }

        public override string ConvertToStreamName(Type tkey, object id)
        {
            return ((ArticleId)id).Value;
        }
    }
}
