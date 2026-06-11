public interface IMessageHandler<T>
{
    bool SendMessage(T message);
}
