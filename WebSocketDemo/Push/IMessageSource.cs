namespace WebSocketDemo.Push
{
    public delegate void MessageHandler(IAuthorizableMessage message);

    public interface IMessageSource
    {
        event MessageHandler OnMessage;
    }
}
