using SimpleEventSourcing.Messaging;

namespace SimpleEventSourcing.WriteModel
{
    public interface IRawStreamEntryFactory
    {
        IRawStreamEntry CreateRawStreamEntry(ISerializer serializer, string streamName, string commitId, int streamRevision, IMessage x);
    }
}
