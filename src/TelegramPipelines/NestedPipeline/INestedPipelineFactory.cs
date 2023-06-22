using Core.TelegramFramework.StatefulPipeline;
using Core.TelegramFramework.TelegramPipeline;
using OneOf;

namespace Core.TelegramFramework.NestedPipeline;

public interface INestedPipelineFactory<TPipelineReturn>
{
    IPipelineExecutor<TPipelineReturn> Create(string nestedPipelineIdentity,
        OneOf<ITelegramPipelineClass<TPipelineReturn>, TelegramPipelineDelegate<TPipelineReturn>> pipeline);
}