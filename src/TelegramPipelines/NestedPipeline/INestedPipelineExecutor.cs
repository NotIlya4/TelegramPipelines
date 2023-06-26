using TelegramPipelines.TelegramPipeline;

namespace TelegramPipelines.NestedPipeline;

public interface INestedPipelineExecutor
{
    Task<TPipelineReturn?> Execute<TPipelineReturn>(string nestedPipelineName,
        TelegramPipelineDelegate<TPipelineReturn> pipeline);

    Task<TPipelineReturn?> Execute<TPipelineReturn>(string nestedPipelineName,
        ITelegramPipelineClass<TPipelineReturn> pipeline);

    Task<TPipelineReturn?> Execute<TPipelineReturn>(ITelegramPipelineClass<TPipelineReturn> pipeline);

    Task Abort(string nestedPipelineName);
}