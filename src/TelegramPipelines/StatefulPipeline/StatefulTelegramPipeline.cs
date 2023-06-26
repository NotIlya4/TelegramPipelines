using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramPipelines.Abstractions;
using TelegramPipelines.NestedPipeline;
using TelegramPipelines.TelegramPipeline;

namespace TelegramPipelines.StatefulPipeline;

public record StatefulTelegramPipeline<TPipelineReturn> : IWrappedTelegramPipeline<TPipelineReturn>
{
    public TelegramPipelineIdentity Identity { get; }
    public TelegramPipelineDelegate<TPipelineReturn> Pipeline { get; }
    public IRecursiveLocalStorage Storage { get; }
    public TelegramRequestContext TelegramRequestContext { get; }
    public NestedPipelineExecutor<TPipelineReturn> NestedPipelineExecutor { get; }
    private readonly IRecursiveLocalStorageFactory _storageFactory;

    public StatefulTelegramPipeline(TelegramPipelineIdentity identity, TelegramPipelineDelegate<TPipelineReturn> pipeline, IRecursiveLocalStorage storage, TelegramRequestContext telegramRequestContext, IRecursiveLocalStorageFactory storageFactory)
    {
        Identity = identity;
        Pipeline = pipeline;
        Storage = storage;
        TelegramRequestContext = telegramRequestContext;
        _storageFactory = storageFactory;
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
            await Storage.ClearStorageAndAllItsChildren();
            throw;
        }

        if (result is not null)
        {
            await Storage.ClearStorageAndAllItsChildren();
        }

        return result;
    }
    
    public async Task Abort()
    {
        await Storage.ClearStorageAndAllItsChildren();
    }

    public async Task<StatefulTelegramPipeline<TChildReturn>> CreateChild<TChildReturn>(string childPipelineName,
        TelegramPipelineDelegate<TChildReturn> pipeline)
    {
        TelegramPipelineIdentity childIdentity = Identity.CreateChild(childPipelineName);
        IRecursiveLocalStorage childStorage = await _storageFactory.GetOrCreateStorage(childIdentity);
        await Storage.AddChildStorage(childStorage);
        return new StatefulTelegramPipeline<TChildReturn>(childIdentity, pipeline, childStorage, TelegramRequestContext, _storageFactory);
    }
}

public interface IWrappedTelegramPipeline<TPipelineReturn>
{
    Task<TPipelineReturn?> Execute();
    Task Abort();
}