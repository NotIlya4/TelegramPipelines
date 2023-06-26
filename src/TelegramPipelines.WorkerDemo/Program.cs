using NotIlya.Extensions.Configuration;
using NotIlya.Extensions.Redis;
using NotIlya.Extensions.Serilog;
using Telegram.Bot;
using TelegramPipelines.Demo.Core;
using TelegramPipelines.Entry;
using TelegramPipelines.RedisLocalStorage;
using TelegramPipelines.TelegramPipeline;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(builder => builder.AddUserSecrets<Program>())
    .ConfigureServices((context, services) =>
    {
        string redisConn = context.Configuration.GetRedisConnectionString("Redis");
        string telegramApiKey = context.Configuration.GetRequiredValue("TelegramApiKey");

        services.AddTelegramPipelinesWorker();
        services.AddTelegramMainPipelineDelegate(async context =>
        {
            await context.NestedPipelineExecutor.Execute("biba", new NumbersAccumulatorPipeline("bibochka"));
            return null;
        });
        services.AddRecursiveLocalStorageFactory<RedisRecursiveLocalStorageFactory>();
        
        services.AddSerilog(context.Configuration.GetAddSerilogOptions("Serilog"));
        services.AddSingleton(new TelegramOptions(redisConn, telegramApiKey));
        services.AddSingleton<ITelegramBotClient>(s => new TelegramBotClient(s.GetRequiredService<TelegramOptions>().TelegramApiKey));
        services.AddRedis(redisConn);
    })
    .Build();

host.Run();