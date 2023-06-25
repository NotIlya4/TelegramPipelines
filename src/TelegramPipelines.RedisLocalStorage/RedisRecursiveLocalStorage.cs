using StackExchange.Redis.Extensions.Core.Abstractions;
using TelegramPipelines.Abstractions;

namespace TelegramPipelines.RedisLocalStorage;

public class RedisRecursiveLocalStorage : IRecursiveLocalStorage
{
    private readonly RedisPrimitiveStorage _primitiveStorage;
    private readonly RedisChildStorageRepository _repository;

    private RedisRecursiveLocalStorage(RedisPrimitiveStorage primitiveStorage, RedisChildStorageRepository repository, TelegramPipelineIdentity storageIdentity)
    {
        _primitiveStorage = primitiveStorage;
        _repository = repository;
        StorageIdentity = storageIdentity;
    }

    public static async Task<RedisRecursiveLocalStorage> Create(IRedisDatabase redis, TelegramPipelineIdentity storageIdentity)
    {
        var primitiveStorage = await RedisPrimitiveStorage.Create(redis, storageIdentity.ColonConcat());
        var storageRepository = new RedisChildStorageRepository(primitiveStorage);

        return new RedisRecursiveLocalStorage(primitiveStorage, storageRepository, storageIdentity);
    }

    public TelegramPipelineIdentity StorageIdentity { get; }
    
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

    public async Task ClearStorageAndAllItsChildren()
    {
        await _repository.ClearStorageRecursive();
    }

    public async Task AddChildStorage(IRecursiveLocalStorage newChildStorage)
    {
        await _repository.AddChild(newChildStorage.StorageIdentity);
    }
}