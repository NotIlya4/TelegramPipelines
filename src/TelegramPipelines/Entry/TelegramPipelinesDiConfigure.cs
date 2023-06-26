using Microsoft.Extensions.DependencyInjection;
using TelegramPipelines.Abstractions;
using TelegramPipelines.TelegramPipeline;

namespace TelegramPipelines.Entry;

public class TelegramPipelinesDiConfigure
{
    private readonly IServiceCollection _services;

    public TelegramPipelinesDiConfigure(IServiceCollection services)
    {
        _services = services;
    }

    public TelegramPipelinesDiConfigure AddTelegramMainPipelineDelegate(
        TelegramPipelineDelegate<MainPipelineFinished> mainPipelineDelegate)
    {
        _services.AddTransient(s => WorkerMainPipeline.FromDelegate(mainPipelineDelegate, s));

        return this;
    }

    public TelegramPipelinesDiConfigure AddTelegramMainPipeline(
        Func<IServiceProvider, ITelegramPipelineClass<MainPipelineFinished>> mainPipelineFactory)
    {
        _services.AddTransient(mainPipelineFactory);
        _services.AddTransient(s => WorkerMainPipeline.FromGeneric<ITelegramPipelineClass<MainPipelineFinished>>(s));

        return this;
    }

    public TelegramPipelinesDiConfigure AddTelegramMainPipeline(Type mainPipelineType)
    {
        _services.AddTransient(mainPipelineType);
        _services.AddTransient(s => WorkerMainPipeline.FromType(mainPipelineType, s));

        return this;
    }

    public TelegramPipelinesDiConfigure AddTelegramMainPipeline<TMainPipeline>()
    {
        return AddTelegramMainPipeline(typeof(TMainPipeline));
    }

    public TelegramPipelinesDiConfigure AddRecursiveLocalStorageFactory(
        Func<IServiceProvider, IRecursiveLocalStorageFactory> storageFactoryFactoryMethod)
    {
        _services.AddSingleton<IRecursiveLocalStorageFactory>(storageFactoryFactoryMethod);

        return this;
    }

    public TelegramPipelinesDiConfigure AddRecursiveLocalStorageFactory(Type recursiveLocalStorageFactoryType)
    {
        _services.AddSingleton(recursiveLocalStorageFactoryType);
        _services.AddSingleton<IRecursiveLocalStorageFactory>(s =>
            (s.GetRequiredService(recursiveLocalStorageFactoryType) as IRecursiveLocalStorageFactory)!);
        
        return this;
    }

    public TelegramPipelinesDiConfigure AddRecursiveLocalStorageFactory<TRecursiveLocalStorageFactory>()
        where TRecursiveLocalStorageFactory : IRecursiveLocalStorageFactory
    {
        return AddRecursiveLocalStorageFactory(typeof(TRecursiveLocalStorageFactory));
    }
}