using SimpleEventSourcing.Messaging;
using SimpleEventSourcing.WriteModel;

namespace SimpleEventSourcing.EntityFramework.WriteModel
{
    public class RawStreamEntryFactory : IRawStreamEntryFactory
    {
        public IRawStreamEntry CreateRawStreamEntry(ISerializer serializer, string streamName, string commitId, int streamRevision, IMessage x)
        {
            return new RawStreamEntry
            {
                Headers = serializer.Serialize(x.Headers),
                MessageId = x.MessageId,
                Payload = serializer.Serialize<object>(x.Body),
                PayloadType = serializer.Binder.BindToName(x.Body.GetType()),
                StreamName = streamName,
                StreamRevision = streamRevision,
                Group = x.Headers?.ContainsKey(MessageConstants.GroupKey) == true ? (string)x.Headers[MessageConstants.GroupKey] : null,
                Category = x.Headers?.ContainsKey(MessageConstants.CategoryKey) == true ? (string)x.Headers[MessageConstants.CategoryKey] : null,
                DateTime = x.DateTime,
                CommitId = commitId
            };
        }
    }
}
