using System;

namespace SimpleEventSourcing.State
{
    public abstract class StreamState<TStreamState> : SynchronousEventSourcedState<TStreamState>, IStreamState<TStreamState>
        where TStreamState : StreamState<TStreamState>, new()
    {
        public string StreamName { get; protected set; }

        public virtual string ConvertToStreamName(Type tkey, object id)
        {
            if (tkey == typeof(string))
            {
                return (string)id;
            }

            return id.ToString();
        }

        public virtual object ConvertFromStreamName(Type tkey, string streamName)
        {
            if (tkey == typeof(string))
            {
                return streamName;
            }
            else if (tkey == typeof(Guid))
            {
                return Guid.Parse(streamName);
            }

            throw new NotSupportedException($"{streamName} cannot be converted to type '{tkey.FullName}'");
        }
    }
}
