using TelegramPipelines.Abstractions;
using TelegramPipelines.StatefulPipeline;
using TelegramPipelines.TelegramPipeline;

namespace TelegramPipelines.NestedPipeline;

public class NestedPipelineExecutor<TParentReturn> : INestedPipelineExecutor
{
    private readonly StatefulTelegramPipeline<TParentReturn> _parentPipeline;

    public NestedPipelineExecutor(StatefulTelegramPipeline<TParentReturn> parentPipeline)
    {
        _parentPipeline = parentPipeline;
    }

    public async Task<TPipelineReturn?> Execute<TPipelineReturn>(string nestedPipelineName, TelegramPipelineDelegate<TPipelineReturn> pipeline)
    {
        StatefulTelegramPipeline<TPipelineReturn> childPipeline = await _parentPipeline.CreateChild(nestedPipelineName, pipeline);
        return await childPipeline.Execute();
    }

    public async Task<TPipelineReturn?> Execute<TPipelineReturn>(string nestedPipelineName, ITelegramPipelineClass<TPipelineReturn> pipeline)
    {
        return await Execute(nestedPipelineName, pipeline.Execute);
    }

    public async Task<TPipelineReturn?> Execute<TPipelineReturn>(ITelegramPipelineClass<TPipelineReturn> pipeline)
    {
        return await Execute(pipeline.GetType().FullName!, pipeline);
    }

    public async Task Abort(string nestedPipelineName)
    {
        var pipelineToAbort = await _parentPipeline.CreateChild(nestedPipelineName, async _ => "");
        await pipelineToAbort.Abort();
    }
}