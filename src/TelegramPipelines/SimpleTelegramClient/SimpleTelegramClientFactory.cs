using Telegram.Bot;

namespace Core.TelegramFramework.SimpleTelegramClient;

public class SimpleTelegramClientFactory
{
    private readonly TelegramBotClient _client;

    public SimpleTelegramClientFactory(TelegramBotClient client)
    {
        _client = client;
    }

    public SimpleTelegramClient Create(long chatId)
    {
        return new SimpleTelegramClient(_client, chatId);
    }
}