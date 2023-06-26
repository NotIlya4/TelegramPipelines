using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NotIlya.Extensions.Redis;
using StackExchange.Redis;

namespace TelegramPipelines.UnitTests;

[CollectionDefinition(nameof(AppFixture))]
public class AppFixture : ICollectionFixture<AppFixture>
{
    public IServiceProvider Services { get; }

    public AppFixture()
    {
        var config = new ConfigurationManager();
        config["Redis:Server"] = "localhost";
        config["Redis:allowAdmin"] = "true";
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddRedis(config.GetRedisConnectionString("Redis"));
        Services = services.BuildServiceProvider();
        Services.GetRequiredService<IDatabase>().Multiplexer.GetServers()[0].FlushDatabase();
    }
}