using Microsoft.Extensions.DependencyInjection;
using TelegramPipelines.Abstractions;
using TelegramPipelines.TelegramPipeline;

namespace TelegramPipelines.Entry;

public static class DiExtensions
{
    public static TelegramPipelinesDiConfigure AddTelegramPipelinesWorker(this IServiceCollection services)
    {
        services.AddHostedService<TelegramPipelinesWorker>();

        return new TelegramPipelinesDiConfigure(services);
    }
}