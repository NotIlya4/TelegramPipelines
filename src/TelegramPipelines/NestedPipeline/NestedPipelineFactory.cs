using Core.TelegramFramework.StatefulPipeline;
using Core.TelegramFramework.TelegramPipeline;
using OneOf;

namespace Core.TelegramFramework.NestedPipeline;

public class NestedPipelineFactory<TPipelineReturn> : INestedPipelineFactory<TPipelineReturn>
{
    private readonly StatefulTelegramPipeline<TPipelineReturn> _parentPipeline;

    public NestedPipelineFactory(StatefulTelegramPipeline<TPipelineReturn> parentPipeline)
    {
        _parentPipeline = parentPipeline;
    }

    public IPipelineExecutor<TPipelineReturn> Create(string nestedPipelineIdentity,
        OneOf<ITelegramPipelineClass<TPipelineReturn>, TelegramPipelineDelegate<TPipelineReturn>> pipeline)
    {
        IPipelineLocalStorageMaster nestedStorage = _parentPipeline.StorageMaster.CreateChild(nestedPipelineIdentity);

        return _parentPipeline.With(nestedStorage);
    }
}