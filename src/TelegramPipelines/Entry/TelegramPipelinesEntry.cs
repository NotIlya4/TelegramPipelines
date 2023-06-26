using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramPipelines.Abstractions;
using TelegramPipelines.NestedPipeline;
using TelegramPipelines.StatefulPipeline;
using TelegramPipelines.TelegramPipeline;

namespace TelegramPipelines.Entry;

public class TelegramPipelinesEntry
{
    private readonly IRecursiveLocalStorageFactory _recursiveLocalStorageFactory;

    public TelegramPipelinesEntry(IRecursiveLocalStorageFactory recursiveLocalStorageFactory)
    {
        _recursiveLocalStorageFactory = recursiveLocalStorageFactory;
    }

    public async Task<TPipelineReturn?> Execute<TPipelineReturn>(ITelegramBotClient telegramBotClient, Update update,
        TelegramPipelineDelegate<TPipelineReturn> mainPipeline)
    {
        long chatId = update switch
        {
            { Message: not null } => update.Message.Chat.Id,
            { EditedMessage: not null } => update.EditedMessage.Chat.Id,
            { ChannelPost: not null } => update.ChannelPost.Chat.Id,
            { EditedChannelPost: not null } => update.EditedChannelPost.Chat.Id,
            _ => -1
        };

        var identity = new TelegramPipelineIdentity(chatId, new List<string>() { "main" });
        
        var statefulPipeline = new StatefulTelegramPipeline<TPipelineReturn>(
            identity,
            mainPipeline,
            await _recursiveLocalStorageFactory.GetOrCreateStorage(identity),
            new TelegramRequestContext(telegramBotClient, update), 
            _recursiveLocalStorageFactory);

        return await statefulPipeline.Execute();
    }

    public async Task<TPipelineReturn?> Execute<TPipelineReturn>(ITelegramBotClient telegramBotClient, Update update,
        ITelegramPipelineClass<TPipelineReturn> mainPipeline)
    {
        return await Execute(telegramBotClient, update, mainPipeline.Execute);
    }
}