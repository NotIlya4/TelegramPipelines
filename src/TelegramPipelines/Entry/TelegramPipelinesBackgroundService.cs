using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis.Extensions.Core.Abstractions;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramPipelines.Abstractions;
using TelegramPipelines.RedisLocalStorage;
using TelegramPipelines.TelegramPipeline;

namespace TelegramPipelines.Entry;

public class TelegramPipelinesBackgroundService : BackgroundService
{
    private readonly IRecursiveLocalStorageFactory _storageFactory;
    private readonly ILogger<TelegramPipelinesBackgroundService> _logger;
    private readonly TelegramPipelineDelegate<MainPipelineFinished> _mainPipeline;

    public TelegramPipelinesBackgroundService(
        ITelegramBotClient bot, 
        IRecursiveLocalStorageFactory storageFactory, 
        ILogger<TelegramPipelinesBackgroundService> logger, 
        TelegramPipelineDelegate<MainPipelineFinished> mainPipeline)
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
        await entry.Execute(client, update, _mainPipeline);
    }
    
    private async Task Error(ITelegramBotClient client, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogWarning(exception, "Telegram bot error occured");
    }
}