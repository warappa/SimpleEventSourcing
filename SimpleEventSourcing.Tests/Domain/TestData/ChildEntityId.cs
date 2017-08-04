using System;

namespace SimpleEventSourcing.Tests
{
    public class ChildEntityId
    {
        public ChildEntityId(string value)
        {
            Value = value;
        }

        public string Value { get; private set; }

        public static implicit operator string(ChildEntityId value)
        {
            return value?.Value;
        }

        public static implicit operator ChildEntityId(string value)
        {
            return new ChildEntityId(value);
        }

        public static ChildEntityId Generate()
        {
            return new ChildEntityId(Guid.NewGuid().ToString());
        }

        public override bool Equals(object obj)
        {
            if (obj is string str)
            {
                obj = (ChildEntityId)str;
            }

            return (obj as ChildEntityId)?.Value == Value;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return Value?.GetHashCode() ?? 0;
            }
        }
    }
}