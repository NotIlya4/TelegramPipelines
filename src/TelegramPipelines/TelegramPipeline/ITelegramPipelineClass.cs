using OneOf;

namespace Core.TelegramFramework.TelegramPipeline;

public interface ITelegramPipelineClass<TPipelineReturn>
{
    Task<OneOf<TPipelineReturn, NotFinished>> Handle(ITelegramPipelineContext<TPipelineReturn> context);
}

public delegate Task<OneOf<TPipelineReturn, NotFinished>>TelegramPipelineDelegate<TPipelineReturn>(ITelegramPipelineContext<TPipelineReturn> context);