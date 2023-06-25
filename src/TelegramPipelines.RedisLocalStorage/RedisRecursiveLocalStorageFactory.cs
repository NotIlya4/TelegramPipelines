using StackExchange.Redis.Extensions.Core.Abstractions;
using TelegramPipelines.Abstractions;

namespace TelegramPipelines.RedisLocalStorage;

public class RedisRecursiveLocalStorageFactory : IRecursiveLocalStorageFactory
{
    private readonly IRedisClientFactory _redisFactory;

    public RedisRecursiveLocalStorageFactory(IRedisClientFactory redisFactory)
    {
        _redisFactory = redisFactory;
    }


    public async Task<IRecursiveLocalStorage> Create(string storageIdentity)
    {
        return await RedisRecursiveLocalStorage.Create(_redisFactory, storageIdentity);
    }
}