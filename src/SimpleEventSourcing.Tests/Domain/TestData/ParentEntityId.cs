using System;

namespace SimpleEventSourcing.Tests
{
    public class ParentEntityId
    {
        public ParentEntityId(string value)
        {
            Value = value;
        }

        public string Value { get; private set; }

        public static implicit operator string(ParentEntityId value)
        {
            return value?.Value;
        }

        public static implicit operator ParentEntityId(string value)
        {
            return new ParentEntityId(value);
        }

        public static ParentEntityId Generate()
        {
            return new ParentEntityId(Guid.NewGuid().ToString());
        }

        public override bool Equals(object obj)
        {
            return (obj as ParentEntityId)?.Value == Value;
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
