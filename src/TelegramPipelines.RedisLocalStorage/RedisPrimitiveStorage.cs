using Newtonsoft.Json.Linq;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace TelegramPipelines.RedisStorageMaster;

public class RedisPrimitiveStorage
{
    private readonly IRedisDatabase _redis;
    public string StorageIdentifier { get; }

    private RedisPrimitiveStorage(IRedisDatabase redis, string storageIdentifier)
    {
        _redis = redis;
        StorageIdentifier = storageIdentifier;
    }

    public static async Task<RedisPrimitiveStorage> Create(IRedisDatabase redis, string storageIdentifier)
    {
        var instance = new RedisPrimitiveStorage(redis, storageIdentifier);
        await instance.Save("__keep", "keep");
        return instance;
    }

    public async Task Save<T>(string key, T o) where T : class
    {
        JObject storage = await GetStorage();
        storage[key] = JToken.FromObject(o);
        await SaveStorage(storage);
    }

    public async Task<T?> Get<T>(string key) where T : class
    {
        JObject storage = await GetStorage();
        return storage[key]?.ToObject<T>();
    }

    public async Task Remove(string key)
    {
        JObject storage = await GetStorage();
        storage.Remove(key);
        await SaveStorage(storage);
    }

    public async Task DeleteStorage()
    {
        await _redis.RemoveAsync(StorageIdentifier);
    }

    private async Task<JObject> GetStorage()
    {
        return await _redis.GetAsync<JObject>(StorageIdentifier) ?? new JObject();
    }

    private async Task SaveStorage(JObject storage)
    {
        await _redis.AddAsync(StorageIdentifier, storage);
    }
}