using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis.Extensions.Core.Abstractions;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramPipelines.Abstractions;
using TelegramPipelines.RedisLocalStorage;
using TelegramPipelines.TelegramPipeline;

namespace TelegramPipelines.Entry;

public class TelegramPipelinesWorker : BackgroundService
{
    private readonly IRecursiveLocalStorageFactory _storageFactory;
    private readonly ILogger<TelegramPipelinesWorker> _logger;
    private readonly WorkerMainPipeline _mainPipeline;

    public TelegramPipelinesWorker(
        ITelegramBotClient bot, 
        IRecursiveLocalStorageFactory storageFactory, 
        ILogger<TelegramPipelinesWorker> logger, 
        WorkerMainPipeline mainPipeline)
    {
        _storageFactory = storageFactory;
        _logger = logger;
        _mainPipeline = mainPipeline;
        bot.StartReceiving(Update, Error);
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(-1, cancellationToken);
    }
    
    private async Task Update(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
    {
        var entry = new TelegramPipelinesEntry(_storageFactory);
        await entry.Execute(client, update, _mainPipeline.CreateMainPipeline());
    }
    
    private async Task Error(ITelegramBotClient client, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogWarning(exception, "Telegram bot error occured");
    }
}