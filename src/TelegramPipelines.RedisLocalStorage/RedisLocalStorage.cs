using Newtonsoft.Json.Linq;
using StackExchange.Redis.Extensions.Core.Abstractions;
using TelegramPipelines.Abstractions;

namespace TelegramPipelines.RedisStorageMaster;

public class RedisLocalStorage : ILocalStorage
{
    private readonly RedisPrimitiveStorage _primitiveStorage;
    private readonly RedisStorageRepository _repository;
    private readonly IRedisClientFactory _redisFactory;

    private RedisLocalStorage(RedisPrimitiveStorage primitiveStorage, RedisStorageRepository repository, IRedisClientFactory redisFactory)
    {
        _primitiveStorage = primitiveStorage;
        _repository = repository;
        _redisFactory = redisFactory;
    }

    public static async Task<RedisLocalStorage> Create(IRedisClientFactory redisFactory, string storageIdentifier)
    {
        IRedisDatabase redis = redisFactory.GetRedisDatabase();
        var primitiveStorage = await RedisPrimitiveStorage.Create(redis, storageIdentifier);
        var storageRepository = new RedisStorageRepository(redis);

        return new RedisLocalStorage(primitiveStorage, storageRepository, redisFactory);
    }
    
    public async Task Save<T>(string key, T o) where T : class
    {
        await _primitiveStorage.Save(key, o);
    }

    public async Task<T?> Get<T>(string key) where T : class
    {
        return await _primitiveStorage.Get<T>(key);
    }

    public async Task Remove(string key)
    {
        await _primitiveStorage.Remove(key);
    }

    public async Task RemoveStorageAndAllItsChildren()
    {
        await _repository.RemoveStorageAndAllItsChildren(_primitiveStorage.StorageIdentifier);
    }

    public async Task<ILocalStorage> CreateChildren(string childStorageIdentifier)
    {
        await _repository.CreateChildStorage(_primitiveStorage.StorageIdentifier, childStorageIdentifier);

        var childRedisDatabase = _redisFactory.GetDefaultRedisDatabase();
        var childPrimitiveStorage =
            await RedisPrimitiveStorage.Create(childRedisDatabase, childStorageIdentifier);
        var childStorageRepository = new RedisStorageRepository(childRedisDatabase);

        return new RedisLocalStorage(childPrimitiveStorage, childStorageRepository, _redisFactory);
    }
}