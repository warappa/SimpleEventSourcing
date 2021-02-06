using SimpleEventSourcing.State;
using System.Threading.Tasks;

namespace SimpleEventSourcing.UI.ConsoleCore
{
    public class VowelCountState : EventSourcedState<VowelCountState>
    {
        public int ACount { get; set; }
        public int ECount { get; set; }
        public int ICount { get; set; }
        public int OCount { get; set; }
        public int UCount { get; set; }

        private int GetCharCount(char c, string str)
        {
            var count = 0;
            foreach (var character in str)
            {
                if (character == c)
                {
                    count++;
                }
            }
            return count;
        }

        public async Task Apply(TestAggregateCreated @event)
        {
            await Task.Yield();

            ACount += GetCharCount('a', @event.Name);
            ECount += GetCharCount('e', @event.Name);
            ICount += GetCharCount('i', @event.Name);
            OCount += GetCharCount('o', @event.Name);
            UCount += GetCharCount('u', @event.Name);
        }

        public async Task Apply(TestAggregateRename @event)
        {
            await Task.Yield();

            ACount += GetCharCount('a', @event.Name);
            ECount += GetCharCount('e', @event.Name);
            ICount += GetCharCount('i', @event.Name);
            OCount += GetCharCount('o', @event.Name);
            UCount += GetCharCount('u', @event.Name);
        }
    }
}