using Telegram.Bot;

namespace Core.TelegramFramework.SimpleTelegramClient;

public class SimpleTelegramClient : ISimpleTelegramClient
{
    private readonly TelegramBotClient _client;
    private readonly long _chatId;

    public SimpleTelegramClient(TelegramBotClient client, long chatId)
    {
        _client = client;
        _chatId = chatId;
    }

    public async Task SendText(string text, SendTextOptions? options = null)
    {
        if (options is not null)
        {
            await _client.SendTextMessageAsync(_chatId, text, parseMode: options.ParseMode);
            return;
        }

        await _client.SendTextMessageAsync(_chatId, text);
    }
}