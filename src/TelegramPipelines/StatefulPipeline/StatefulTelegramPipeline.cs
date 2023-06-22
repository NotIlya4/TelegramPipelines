using Core.TelegramFramework.NestedPipeline;
using Core.TelegramFramework.SimpleTelegramClient;
using Core.TelegramFramework.TelegramPipeline;
using OneOf;

namespace Core.TelegramFramework.StatefulPipeline;

public record StatefulTelegramPipeline<TPipelineReturn> : IPipelineExecutor<TPipelineReturn>
{
    public ISimpleTelegramClient Bot { get; }
    public UserRequest UserRequest { get; }
    public IPipelineLocalStorageMaster StorageMaster { get; }
    public TelegramPipelineDelegate<TPipelineReturn> Pipeline { get; }
    public NestedPipelineFactory<TPipelineReturn> NestedPipelineFactory { get; }
    public TelegramPipelineContext<TPipelineReturn> PipelineContext => new(Bot, UserRequest, Storage, NestedPipelineFactory);
    public IPipelineLocalStorage Storage => StorageMaster.ToPipelineLocalStorage();

    public StatefulTelegramPipeline(ISimpleTelegramClient bot, UserRequest userRequest, IPipelineLocalStorageMaster storageMaster, TelegramPipelineDelegate<TPipelineReturn> pipeline)
    {
        Bot = bot;
        UserRequest = userRequest;
        StorageMaster = storageMaster;
        Pipeline = pipeline;
        NestedPipelineFactory = new NestedPipelineFactory<TPipelineReturn>(this);
    }

    public async Task<OneOf<TPipelineReturn, NotFinished>> Execute()
    {
        OneOf<TPipelineReturn, NotFinished> result;
        try
        {
            result = await Pipeline(PipelineContext);
        }
        catch (Exception)
        {
            await StorageMaster.Clear();
            throw;
        }

        if (result.IsT1)
        {
            await StorageMaster.Clear();
        }

        return result;
    }
    
    public async Task Abort()
    {
        await StorageMaster.Clear();
    }

    public StatefulTelegramPipeline<TPipelineReturn> With(IPipelineLocalStorageMaster storageMaster)
    {
        return new StatefulTelegramPipeline<TPipelineReturn>(
            Bot, UserRequest, storageMaster, Pipeline);
    }
}

public interface IPipelineExecutor<TPipelineReturn>
{
    Task<OneOf<TPipelineReturn, NotFinished>> Execute();
    Task Abort();
}