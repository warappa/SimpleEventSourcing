using System;

namespace SimpleEventSourcing.WriteModel.InMemory
{
    public class RawStreamEntry : IRawStreamEntry
    {
        public string Category { get; set; }
        public int CheckpointNumber { get; set; }
        public string CommitId { get; set; }
        public DateTime DateTime { get; set; }
        public string Group { get; set; }
        public string Headers { get; set; }
        public string MessageId { get; set; }
        public string Payload { get; set; }
        public string PayloadType { get; set; }
        public string StreamName { get; set; }
        public int StreamRevision { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as RawStreamEntry;
            if (other == null)
            {
                return false;
            }

            return Category == other.Category &&
                CheckpointNumber == other.CheckpointNumber &&
                CommitId == other.CommitId &&
                DateTime == other.DateTime &&
                Group == other.Group &&
                Headers == other.Headers &&
                MessageId == other.MessageId &&
                Payload == other.Payload &&
                PayloadType == other.PayloadType &&
                StreamName == other.StreamName &&
                StreamRevision == other.StreamRevision;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 27;

                hash = (hash * 17) + (Category?.GetHashCode() ?? 0);
                hash = (hash * 17) + CheckpointNumber.GetHashCode();
                hash = (hash * 17) + (CommitId?.GetHashCode() ?? 0);
                hash = (hash * 17) + DateTime.GetHashCode();
                hash = (hash * 17) + (Group?.GetHashCode() ?? 0);
                hash = (hash * 17) + (Headers?.GetHashCode() ?? 0);
                hash = (hash * 17) + (MessageId?.GetHashCode() ?? 0);
                hash = (hash * 17) + (Payload?.GetHashCode() ?? 0);
                hash = (hash * 17) + (PayloadType?.GetHashCode() ?? 0);
                hash = (hash * 17) + (StreamName?.GetHashCode() ?? 0);
                hash = (hash * 17) + StreamRevision.GetHashCode();

                return hash;
            }
        }
    }
}
