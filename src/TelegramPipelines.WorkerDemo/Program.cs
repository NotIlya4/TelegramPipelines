using Newtonsoft.Json.Linq;
using NotIlya.Extensions.Configuration;
using NotIlya.Extensions.Redis;
using NotIlya.Extensions.Serilog;
using Telegram.Bot;
using TelegramPipelines.Demo.Core;
using TelegramPipelines.Entry;
using TelegramPipelines.JObjectLocalStorage;
using TelegramPipelines.RedisLocalStorage;
using TelegramPipelines.TelegramPipeline;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(builder => builder.AddUserSecrets<Program>())
    .ConfigureServices((context, services) =>
    {
        string redisConn = context.Configuration.GetRedisConnectionString("Redis");
        string telegramApiKey = context.Configuration.GetRequiredValue("TelegramApiKey");

        services.AddTelegramPipelinesWorker()
            .AddTelegramMainPipelineDelegate(async context => 
            {
                UserForm? userForm = await context.NestedPipelineExecutor.Execute("userform", new UserFormPipeline());
                if (userForm is not null)
                {
                    await context.TelegramBotClient.SendTextMessageAsync(context.Update.Message!.Chat, $"Your form {userForm}");
                }
                return null;
            })
            .AddRecursiveLocalStorageFactory(s => new JObjectRecursiveLocalStorageFactory(new JObject()));
        
        services.AddSerilog(context.Configuration.GetAddSerilogOptions("Serilog"));
        services.AddSingleton<ITelegramBotClient>(s => new TelegramBotClient(telegramApiKey));
        services.AddRedis(redisConn);
    })
    .Build();

host.Run();