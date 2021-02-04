using System;
using System.Collections.Generic;

namespace Shop.Core.Domain.Shared
{
    public abstract class BaseChildId<T, TAggregateRootId> : IChildId<TAggregateRootId>
        where T : BaseChildId<T, TAggregateRootId>
        where TAggregateRootId : BaseId<TAggregateRootId>
    {
        public static readonly T Empty = Activator.CreateInstance<T>();

        public readonly TAggregateRootId AggregateRootId;
        public readonly string Value;

        string IId.Value => Value;
        TAggregateRootId IChildId<TAggregateRootId>.AggregateRootId => AggregateRootId;

        protected BaseChildId()
        {
            AggregateRootId = Activator.CreateInstance<TAggregateRootId>();
            Value = "";
        }

        protected BaseChildId(string value, TAggregateRootId aggregateRootId)
        {
            Value = value;
            AggregateRootId = aggregateRootId;
        }

        protected static TAggregateRootId CreateAggregateRootId(string aggregateRootId)
        {
            return (TAggregateRootId)Activator.CreateInstance(typeof(TAggregateRootId), aggregateRootId);
        }

        public static T From(TAggregateRootId aggregateRootId, string value)
        {
            return (T)Activator.CreateInstance(typeof(T), value, aggregateRootId);
        }

        public static T From(string aggregateRootId, string value)
        {
            return From(CreateAggregateRootId(aggregateRootId), value);
        }

        public static T Generate(TAggregateRootId aggregateRootId)
        {
            return From(aggregateRootId, Guid.NewGuid().ToString());
        }

        public static T Generate(string aggregateRootId)
        {
            return From(aggregateRootId, Guid.NewGuid().ToString());
        }

        public override bool Equals(object obj)
        {
            var other = obj as T;
            if (other == null)
            {
                return false;
            }

            return Value == other.Value;
        }

        protected static string ConvertToStringId(T value)
        {
            if (value == null)
            {
                return ConvertToStringId(Empty);
            }

            return $"{value.AggregateRootId.Value}|{value.Value}";
        }

        protected static T ConvertToId(string value)
        {
            if (string.IsNullOrEmpty(value))
                return Empty;

            var strs = value.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            if (strs.Length != 2)
                return Empty;

            return (T)Activator.CreateInstance(typeof(T), strs[1], CreateAggregateRootId(strs[0]));
        }

        public override string ToString()
        {
            return ConvertToStringId((T)this);
        }

        public override int GetHashCode()
        {
            var hashCode = -1937772220;
            hashCode = hashCode * -1521134295 + EqualityComparer<TAggregateRootId>.Default.GetHashCode(AggregateRootId);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Value);
            return hashCode;
        }

        public static bool operator ==(BaseChildId<T, TAggregateRootId> left, BaseChildId<T, TAggregateRootId> right)
        {
            return EqualityComparer<BaseChildId<T, TAggregateRootId>>.Default.Equals(left, right);
        }

        public static bool operator !=(BaseChildId<T, TAggregateRootId> left, BaseChildId<T, TAggregateRootId> right)
        {
            return !(left == right);
        }
    }
}
