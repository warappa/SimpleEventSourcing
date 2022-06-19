using System.Collections.Generic;
using System.Linq;

namespace SimpleEventSourcing.WriteModel
{
    public static class RawStreamEntryExtensions
    {
        public static IEnumerable<IRawStreamEntry> WithGroup(this IEnumerable<IRawStreamEntry> entries, string group)
        {
            return entries.Where(x => x.Group == group);
        }

        public static IEnumerable<IRawStreamEntry> WithCategory(this IEnumerable<IRawStreamEntry> entries, string category)
        {
            return entries.Where(x => x.Category == category);
        }

        public static IEnumerable<IRawStreamEntry> WithStreamName(this IEnumerable<IRawStreamEntry> entries, string streamName)
        {
            return entries.Where(x => x.StreamName == streamName);
        }

        public static IEnumerable<IRawStreamEntry> WithPayloadType<T>(this IEnumerable<IRawStreamEntry> entries, ISerializationBinder binder = null)
        {
            binder = binder ?? new VersionedBinder();

            return entries.Where(x => x.PayloadType == binder.BindToName(typeof(T)));
        }

        public static IEnumerable<IRawStreamEntry> WithPayloadTypes<T1, T2>(this IEnumerable<IRawStreamEntry> entries, ISerializationBinder binder = null)
        {
            binder = binder ?? new VersionedBinder();

            var allNames = new[] {
                binder.BindToName(typeof(T1)),
                binder.BindToName(typeof(T2))
            };

            return entries.Where(x => allNames.Contains(x.PayloadType));
        }

        public static IEnumerable<IRawStreamEntry> WithPayloadTypes<T1, T2, T3>(this IEnumerable<IRawStreamEntry> entries, ISerializationBinder binder = null)
        {
            binder = binder ?? new VersionedBinder();

            var allNames = new[] {
                binder.BindToName(typeof(T1)),
                binder.BindToName(typeof(T2)),
                binder.BindToName(typeof(T3))
            };

            return entries.Where(x => allNames.Contains(x.PayloadType));
        }
    }
}
