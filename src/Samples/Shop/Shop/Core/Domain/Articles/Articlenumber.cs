namespace Shop.Core.Domain.Articles
{
    public class Articlenumber
    {
        public static readonly Articlenumber Empty = new("");

        public string Value { get; private set; }

        public Articlenumber(string value)
        {
            Value = value;
        }

        public override bool Equals(object obj)
        {
            var other = obj as Articlenumber;
            if (other == null)
            {
                return false;
            }

            return other.Value == Value;
        }

        public static implicit operator string(Articlenumber value)
        {
            return value?.Value ?? Empty.Value;
        }

        public static implicit operator Articlenumber(string value)
        {
            return new Articlenumber(value);
        }

        public override int GetHashCode()
        {
            return Value?.GetHashCode() ?? 0;
        }
    }
}
