using StackExchange.Redis.Extensions.Core.Abstractions;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramPipelines.Entry;
using TelegramPipelines.RedisLocalStorage;

namespace TelegramPipelines.Demo.Core;

public class TelegramBotWorker : BackgroundService
{
    private readonly IRedisClientFactory _redisFactory;
    private readonly ILogger<TelegramBotWorker> _logger;

    public TelegramBotWorker(TelegramBotClient bot, IRedisClientFactory redisFactory, ILogger<TelegramBotWorker> logger)
    {
        _redisFactory = redisFactory;
        _logger = logger;
        bot.StartReceiving(Update, Error);
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(-1, cancellationToken);
    }
    
    private async Task Update(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Message: {Message}", update.Message?.Text);
        // var entry = new TelegramPipelinesEntry(new RedisRecursiveLocalStorageFactory(_redisFactory));
        // await entry.Execute<string>(client, update, async context => 
        // { 
        //     _logger.LogInformation(context.Update.Message?.Text);
        //     return null;
        // });
    }
    
    private async Task Error(ITelegramBotClient client, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogWarning(exception, "Telegram bot error occured");
    }
}