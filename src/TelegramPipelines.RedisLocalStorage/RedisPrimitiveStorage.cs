using Newtonsoft.Json.Linq;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace TelegramPipelines.RedisLocalStorage;

internal class RedisPrimitiveStorage
{
    public IRedisDatabase Redis { get; }
    public string StorageIdentity { get; }

    private RedisPrimitiveStorage(IRedisDatabase redis, string storageIdentity)
    {
        Redis = redis;
        StorageIdentity = storageIdentity;
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
        await Redis.RemoveAsync(StorageIdentity);
    }

    private async Task<JObject> GetStorage()
    {
        return await Redis.GetAsync<JObject>(StorageIdentity) ?? new JObject();
    }

    private async Task SaveStorage(JObject storage)
    {
        await Redis.AddAsync(StorageIdentity, storage);
    }
}