using Microsoft.Extensions.DependencyInjection;
using TelegramPipelines.Abstractions;
using TelegramPipelines.TelegramPipeline;

namespace TelegramPipelines.Entry;

public static class DiExtensions
{
    public static IServiceCollection AddTelegramPipelinesWorker(this IServiceCollection services)
    {
        services.AddHostedService<TelegramPipelinesWorker>();

        return services;
    }
    
    public static IServiceCollection AddRecursiveLocalStorageFactory<TRecursiveLocalStorageFactory>(this IServiceCollection services) 
        where TRecursiveLocalStorageFactory : IRecursiveLocalStorageFactory
    {
        services.AddRecursiveLocalStorageFactory(typeof(TRecursiveLocalStorageFactory));
        
        return services;
    }
    
    public static IServiceCollection AddRecursiveLocalStorageFactory(this IServiceCollection services, Type recursiveLocalStorageFactoryType)
    {
        services.AddSingleton(recursiveLocalStorageFactoryType);
        services.AddSingleton<IRecursiveLocalStorageFactory>(s =>
            (s.GetRequiredService(recursiveLocalStorageFactoryType) as IRecursiveLocalStorageFactory)!);
        
        return services;
    }

    public static IServiceCollection AddRecursiveLocalStorageFactory(this IServiceCollection services,
        Func<IServiceProvider, IRecursiveLocalStorageFactory> storageFactoryFactoryMethod)
    {
        services.AddSingleton<IRecursiveLocalStorageFactory>(storageFactoryFactoryMethod);
        
        return services;
    }
    
    public static IServiceCollection AddTelegramMainPipeline<TMainPipeline>(this IServiceCollection services) 
        where TMainPipeline : ITelegramPipelineClass<MainPipelineFinished>
    {
        services.AddTelegramMainPipeline(typeof(TMainPipeline));
        
        return services;
    }
    
    public static IServiceCollection AddTelegramMainPipeline(this IServiceCollection services, Type mainPipelineType)
    {
        services.AddTransient(mainPipelineType);
        services.AddTransient(s => WorkerMainPipeline.FromType(mainPipelineType, s));
        
        return services;
    }
    
    public static IServiceCollection AddTelegramMainPipeline(this IServiceCollection services, Func<IServiceProvider, ITelegramPipelineClass<MainPipelineFinished>> mainPipelineFactory)
    {
        services.AddTransient(mainPipelineFactory);
        services.AddTransient(s => WorkerMainPipeline.FromGeneric<ITelegramPipelineClass<MainPipelineFinished>>(s));
        
        return services;
    }
    
    public static IServiceCollection AddTelegramMainPipelineDelegate(this IServiceCollection services, TelegramPipelineDelegate<MainPipelineFinished> mainPipelineDelegate)
    {
        services.AddTransient(s => WorkerMainPipeline.FromDelegate(mainPipelineDelegate, s));
        
        return services;
    }
}