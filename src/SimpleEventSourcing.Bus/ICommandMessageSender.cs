using SimpleEventSourcing.Messaging;

namespace SimpleEventSourcing.Bus
{
    public interface ICommandMessageSender
    {
        void Send<T>(T command)
            where T : IMessage<ICommand>;
    }
}
