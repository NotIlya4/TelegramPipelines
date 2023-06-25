namespace TelegramPipelines.TelegramPipeline;

public interface ITelegramPipelineClass<TPipelineReturn>
{
    Task<TPipelineReturn?> Execute(TelegramPipelineContext context);
}