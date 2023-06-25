using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramPipelines.Abstractions;
using TelegramPipelines.NestedPipeline;

namespace TelegramPipelines.TelegramPipeline;

public record TelegramPipelineContext(
    TelegramPipelineIdentity PipelineIdentity,
    ITelegramPipelineLocalStorage LocalStorage,
    INestedPipelineExecutor NestedPipelineExecutor,
    Update Update,
    TelegramBotClient TelegramBotClient)
{
    public TelegramPipelineContext(
        TelegramPipelineIdentity pipelineIdentity,
        ITelegramPipelineLocalStorage localStorage,
        INestedPipelineExecutor nestedPipelineExecutor,
        TelegramRequestContext telegramRequestContext) : this(
        pipelineIdentity,
        localStorage,
        nestedPipelineExecutor,
        telegramRequestContext.Update,
        telegramRequestContext.TelegramBotClient)
    {
        
    }
};