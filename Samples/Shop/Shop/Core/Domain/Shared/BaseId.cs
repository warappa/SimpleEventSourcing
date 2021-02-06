using System;
using System.Collections.Generic;

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
            {
                return false;
            }

            return Value == other.Value;
        }

        public override int GetHashCode()
        {
            var hashCode = -1951975302;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Value);
            return hashCode;
        }

        public static implicit operator string(BaseId<T> value)
        {
            return value?.Value;
        }

        public static implicit operator BaseId<T>(string value)
        {
            return string.IsNullOrEmpty(value) ? null : new BaseId<T>(value);
        }

        public static bool operator ==(BaseId<T> left, BaseId<T> right)
        {
            return EqualityComparer<BaseId<T>>.Default.Equals(left, right);
        }

        public static bool operator !=(BaseId<T> left, BaseId<T> right)
        {
            return !(left == right);
        }
    }
}
