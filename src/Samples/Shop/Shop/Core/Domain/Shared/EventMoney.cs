using SimpleEventSourcing.WriteModel;

namespace Shop.Core.Domain.Shared
{
    [Versioned("EventMoney", 0)]
    public class EventMoney
    {
        public decimal Value { get; set; }
        public string IsoCode { get; set; }

        public EventMoney(decimal value, string isoCode)
        {
            Value = value;
            IsoCode = isoCode;
        }

        public static implicit operator EventMoney(Money value)
        {
            return new EventMoney(value.Value, value.IsoCode);
        }

        public static implicit operator Money(EventMoney value)
        {
            return new Money(value.Value, value.IsoCode);
        }
    }
}
