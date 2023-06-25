using StackExchange.Redis.Extensions.Core.Abstractions;
using TelegramPipelines.Abstractions;

namespace TelegramPipelines.RedisLocalStorage;

public class RedisRecursiveLocalStorage : IRecursiveLocalStorage
{
    private readonly RedisPrimitiveStorage _primitiveStorage;
    private readonly RedisStorageRepository _repository;
    private readonly IRedisClientFactory _redisFactory;

    private RedisRecursiveLocalStorage(RedisPrimitiveStorage primitiveStorage, RedisStorageRepository repository, IRedisClientFactory redisFactory)
    {
        _primitiveStorage = primitiveStorage;
        _repository = repository;
        _redisFactory = redisFactory;
    }

    public static async Task<RedisRecursiveLocalStorage> Create(IRedisClientFactory redisFactory, string storageIdentifier)
    {
        IRedisDatabase redis = redisFactory.GetRedisDatabase();
        var primitiveStorage = await RedisPrimitiveStorage.Create(redis, storageIdentifier);
        var storageRepository = new RedisStorageRepository(redis);

        return new RedisRecursiveLocalStorage(primitiveStorage, storageRepository, redisFactory);
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

    public string StorageIdentity => _primitiveStorage.StorageIdentifier;

    public async Task RemoveStorageAndAllItsChildren()
    {
        await _repository.RemoveStorageAndAllItsChildren(_primitiveStorage.StorageIdentifier);
    }

    public async Task<IRecursiveLocalStorage> GetOrCreateChild(string childStorageIdentifier)
    {
        await _repository.CreateChildStorage(_primitiveStorage.StorageIdentifier, childStorageIdentifier);

        var childRedisDatabase = _redisFactory.GetDefaultRedisDatabase();
        var childPrimitiveStorage =
            await RedisPrimitiveStorage.Create(childRedisDatabase, childStorageIdentifier);
        var childStorageRepository = new RedisStorageRepository(childRedisDatabase);

        return new RedisRecursiveLocalStorage(childPrimitiveStorage, childStorageRepository, _redisFactory);
    }
}