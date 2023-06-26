using Microsoft.Extensions.DependencyInjection;
using TelegramPipelines.TelegramPipeline;

namespace TelegramPipelines.Entry;

public class WorkerMainPipeline
{
    private readonly IServiceProvider _services;
    private Func<IServiceProvider, TelegramPipelineDelegate<MainPipelineFinished>> MainPipelineFactory { get; }
    
    public WorkerMainPipeline(Func<IServiceProvider, TelegramPipelineDelegate<MainPipelineFinished>> mainPipelineFactory, IServiceProvider services)
    {
        _services = services;
        MainPipelineFactory = mainPipelineFactory;
    }

    public static WorkerMainPipeline FromGeneric<TMainPipeline>(IServiceProvider services)
        where TMainPipeline : ITelegramPipelineClass<MainPipelineFinished>
    {
        return FromType(typeof(TMainPipeline), services);
    }

    public static WorkerMainPipeline FromType(Type mainPipelineType, IServiceProvider services)
    {
        return new WorkerMainPipeline(s =>
        {
            ITelegramPipelineClass<MainPipelineFinished> mainPipeline =
                (s.GetRequiredService(mainPipelineType) as ITelegramPipelineClass<MainPipelineFinished>)!;

            return mainPipeline.Execute;
        }, services);
    }

    public static WorkerMainPipeline FromDelegate(TelegramPipelineDelegate<MainPipelineFinished> del, IServiceProvider services)
    {
        return new WorkerMainPipeline(s => del, services);
    }

    public TelegramPipelineDelegate<MainPipelineFinished> CreateMainPipeline()
    {
        return MainPipelineFactory(_services);
    }
}