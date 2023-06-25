namespace TelegramPipelines.TelegramPipeline;

public interface ITelegramPipelineClass<TPipelineReturn>
{
    Task<TPipelineReturn?> Handle(TelegramPipelineContext context);
}