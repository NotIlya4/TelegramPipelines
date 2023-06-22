namespace Core.TelegramFramework.SimpleTelegramClient;

public interface ISimpleTelegramClient
{
    Task SendText(string text, SendTextOptions? options = null);
}