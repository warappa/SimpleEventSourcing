using System;

namespace Shop.Core.Domain.Shared
{
    public class BaseId<T> : IId
        where T : BaseId<T>
    {
        public static readonly T Empty = Activator.CreateInstance<T>();

        public readonly string Value;

        string IId.Value => Value;

        public static T From(string value)
        {
            return (T)Activator.CreateInstance(typeof(T), value);
        }

        protected BaseId() { }
        public BaseId(string value)
        {
            Value = value;
        }

        public static T Generate()
        {
            return From(Guid.NewGuid().ToString());
        }

        public override bool Equals(object obj)
        {
            var other = obj as T;
            if (obj == null)
                return false;

            return Value == other.Value;
        }

        public static implicit operator string(BaseId<T> value)
        {
            return value?.Value;
        }

        public static implicit operator BaseId<T>(string value)
        {
            return string.IsNullOrEmpty(value) ? null : new BaseId<T>(value);
        }
    }
}
