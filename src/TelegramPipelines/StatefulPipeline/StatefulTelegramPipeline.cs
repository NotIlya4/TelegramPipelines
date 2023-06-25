using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramPipelines.Abstractions;
using TelegramPipelines.LocalStorage;
using TelegramPipelines.NestedPipeline;
using TelegramPipelines.TelegramPipeline;

namespace TelegramPipelines.StatefulPipeline;

public record StatefulTelegramPipeline<TPipelineReturn> : IWrappedTelegramPipeline<TPipelineReturn>
{
    public TelegramPipelineIdentity Identity { get; }
    public TelegramPipelineDelegate<TPipelineReturn> Pipeline { get; }
    public PipelineLocalStorage Storage { get; }
    public TelegramRequestContext TelegramRequestContext { get; }
    public NestedPipelineExecutor<TPipelineReturn> NestedPipelineExecutor { get; }

    public StatefulTelegramPipeline(TelegramPipelineIdentity identity, TelegramPipelineDelegate<TPipelineReturn> pipeline, PipelineLocalStorage storage, TelegramRequestContext telegramRequestContext)
    {
        Identity = identity;
        Pipeline = pipeline;
        Storage = storage;
        TelegramRequestContext = telegramRequestContext;
        NestedPipelineExecutor = new NestedPipelineExecutor<TPipelineReturn>(this);
    }

    public async Task<TPipelineReturn?> Execute()
    {
        TPipelineReturn? result;
        try
        {
            result = await Pipeline(new TelegramPipelineContext(Identity, Storage, NestedPipelineExecutor, TelegramRequestContext));
        }
        catch (Exception)
        {
            await Storage.RemoveStorageAndAllItsChildren();
            throw;
        }

        if (result is null)
        {
            await Storage.RemoveStorageAndAllItsChildren();
        }

        return result;
    }
    
    public async Task Abort()
    {
        await Storage.RemoveStorageAndAllItsChildren();
    }

    public async Task<StatefulTelegramPipeline<TChildReturn>> CreateChild<TChildReturn>(string childPipelineName,
        TelegramPipelineDelegate<TChildReturn> pipeline)
    {
        TelegramPipelineIdentity childIdentity = Identity.CreateChild(childPipelineName);
        PipelineLocalStorage childStorage = await Storage.CreateChild(childIdentity);
        return new StatefulTelegramPipeline<TChildReturn>(childIdentity, pipeline, childStorage, TelegramRequestContext);
    }
}

public interface IWrappedTelegramPipeline<TPipelineReturn>
{
    Task<TPipelineReturn?> Execute();
    Task Abort();
}