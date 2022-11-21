using SimpleEventSourcing.ReadModel;
using SimpleEventSourcing.Tests.ReadModel;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SimpleEventSourcing.EntityFrameworkCore.Tests.Storage
{
    [Table("CompoundKeyTestEntity")]
    public class CompoundKeyTestEntity : ICompoundKeyTestEntity
    {
        public Guid Key1 { get; set; }
        public Guid Key2 { get; set; }

        public string Value { get; set; }
        public string Streamname
        {
            get => $"{Key1}/{Key2}"; set
            {
                var strs = value.Split('/');
                Key1 = Guid.Parse(strs[0]);
                Key2 = Guid.Parse(strs[1]);
            }
        }
        object IReadModelBase.Id { get => new { Key1, Key2 }; set => throw new NotSupportedException(); }

        public override bool Equals(object obj)
        {
            var other = obj as CompoundKeyTestEntity;

            if (other is null)
            {
                return false;
            }

            return other.Key1.Equals(Key1) &&
                other.Key2.Equals(Key2);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Key1, Key2).GetHashCode();
        }
    }
}
