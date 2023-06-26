using Microsoft.Extensions.DependencyInjection;
using TelegramPipelines.Abstractions;

namespace TelegramPipelines.Entry;

public static class DiExtensions
{
    public IServiceCollection AddTelegramPipelinesBackgroundService(this IServiceCollection services, IRecursiveLocalStorageFactory factory)
}