using Core.TelegramFramework.NestedPipeline;
using Core.TelegramFramework.SimpleTelegramClient;
using Core.TelegramFramework.StatefulPipeline;

namespace Core.TelegramFramework.TelegramPipeline;

public interface ITelegramPipelineContext<TPipelineReturn>
{
    ISimpleTelegramClient Bot { get; init; }
    UserRequest UserRequest { get; init; }
    IPipelineLocalStorage Storage { get; init; }
    INestedPipelineFactory<TPipelineReturn> NestedPipelineFactory { get; init; }
}

public record TelegramPipelineContext<TPipelineReturn>(
        ISimpleTelegramClient Bot,
        UserRequest UserRequest,
        IPipelineLocalStorage Storage,
        INestedPipelineFactory<TPipelineReturn> NestedPipelineFactory)
    : ITelegramPipelineContext<TPipelineReturn>
{
    public TelegramPipelineContext<TPipelineReturn> With(IPipelineLocalStorage newStorage)
    {
        return new TelegramPipelineContext<TPipelineReturn>(
            Bot, UserRequest, newStorage, NestedPipelineFactory);
    }
};

public record UserRequest(string TextMessage);