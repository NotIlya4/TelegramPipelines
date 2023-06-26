using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core.Abstractions;
using TelegramPipelines.Abstractions;

namespace TelegramPipelines.RedisLocalStorage;

public class RedisRecursiveLocalStorageFactory : IRecursiveLocalStorageFactory
{
    private readonly ConnectionMultiplexer _multiplexer;

    public RedisRecursiveLocalStorageFactory(ConnectionMultiplexer multiplexer)
    {
        _multiplexer = multiplexer;
    }


    public async Task<IRecursiveLocalStorage> GetOrCreateStorage(TelegramPipelineIdentity storageIdentity)
    {
        return await RedisRecursiveLocalStorage.Create(_multiplexer.GetDatabase(), storageIdentity);
    }
}