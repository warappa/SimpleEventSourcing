using System;

namespace SimpleEventSourcing.WriteModel
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class VersionedAttribute : Attribute
    {
        public int Version { get; set; }

        public string Identifier { get; set; }

        public VersionedAttribute(string identifier, int version = 0)
        {
            Identifier = identifier;
            Version = version;
        }
    }
}
