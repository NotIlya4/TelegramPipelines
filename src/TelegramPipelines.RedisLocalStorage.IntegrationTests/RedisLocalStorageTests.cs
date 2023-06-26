using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core.Abstractions;
using TelegramPipelines.Abstractions;
using TelegramPipelines.RedisLocalStorage;

namespace TelegramPipelines.UnitTests;

[Collection(nameof(AppFixture))]
public class RedisLocalStorageTests : IDisposable
{
    private readonly IDatabase _redis;
    private readonly IServiceScope _scope;
    private readonly RedisRecursiveLocalStorage _storage;
    private readonly RedisRecursiveLocalStorageFactory _storageFactory;
    private readonly TelegramPipelineIdentity _identity = new(100, new [] { "main" });

    public RedisLocalStorageTests(AppFixture fixture)
    {
        _scope = fixture.Services.CreateScope();
        _redis = _scope.ServiceProvider.GetRequiredService<IDatabase>();
        _storageFactory = new RedisRecursiveLocalStorageFactory(_scope.ServiceProvider.GetRequiredService<ConnectionMultiplexer>());
        _storage = (_storageFactory.GetOrCreateStorage(_identity).GetAwaiter().GetResult() as RedisRecursiveLocalStorage)!;
    }

    [Fact]
    public async Task Save_EmptyRedisStorage_RedisStorageWithValue()
    {
        TestClass expect = new TestClass(10, "asd");
        await _storage.Save("asd", expect);
        TestClass? result = (await Get(_identity.ColonConcat()))["asd"]?.ToObject<TestClass>();

        Assert.Equal(expect, result);
    }

    private async Task<JObject> Get(string key)
    {
        string? raw = await _redis.StringGetAsync(key);
        return raw is not null ? JObject.Parse(raw) : new JObject();
    }

    [Fact]
    public async Task Get_RedisStorageWithValue_ReturnThatValue()
    {
        TestClass expect = new TestClass(10, "asd");
        await _storage.Save("asd", expect);
        TestClass? result = await _storage.Get<TestClass>("asd");

        Assert.Equal(expect, result);
    }

    [Fact]
    public async Task Remove_RedisWithValue_RemoveThatValue()
    {
        await _storage.Save("asd", new TestClass(10, "asd"));
        await _storage.Remove("asd");
        TestClass? result = await _storage.Get<TestClass>("asd");

        Assert.Null(result);
    }

    [Fact]
    public async Task Create_EmptyRedis_AfterCreateInStorageIdentityKeyKeepValue()
    {
        var expect = new JObject()
        {
            ["__keep"] = "keep"
        };
        await RedisRecursiveLocalStorage.Create(_redis, _identity);
        JObject? result = await Get(_identity.ColonConcat());

        Assert.Equal(expect, result);
    }

    [Fact]
    public async Task CreateChild_EmptyStorage_AddChildrenPropertyInStorage()
    {
        var expect = new List<string>() { "100:main:a", "100:main:b" };
        IRecursiveLocalStorage childA = await _storageFactory.GetOrCreateStorage(_identity.CreateChild("a"));
        IRecursiveLocalStorage childB = await _storageFactory.GetOrCreateStorage(_identity.CreateChild("b"));

        await _storage.AddChildStorage(childA);
        await _storage.AddChildStorage(childB);

        var result = (await Get(_identity.ColonConcat()))?["__children"]?.ToObject<List<string>>();

        Assert.Equal(expect, result);
    }

    [Fact]
    public async Task RemoveStorageAndAllItsChildren_StorageGraph_RemoveWholeGraph()
    {
        IRecursiveLocalStorage child1 = await _storageFactory.GetOrCreateStorage(_identity.CreateChild("child1"));
        IRecursiveLocalStorage child2 = await _storageFactory.GetOrCreateStorage(_identity.CreateChild("child2"));
        IRecursiveLocalStorage child1child1 = await _storageFactory.GetOrCreateStorage(_identity.CreateChild("child1child1"));
        IRecursiveLocalStorage child1child2 = await _storageFactory.GetOrCreateStorage(_identity.CreateChild("child1child2"));

        await _storage.AddChildStorage(child1);
        await _storage.AddChildStorage(child2);
        await child1.AddChildStorage(child1child1);
        await child1.AddChildStorage(child1child2);

        await _storage.ClearStorageAndAllItsChildren();
        List<RedisKey> keys = _redis.Multiplexer.GetServers()[0].Keys().ToList();
        
        Assert.Empty(keys);
    }

    public void Dispose()
    {
        _redis.Multiplexer.GetServers()[0].FlushDatabaseAsync().GetAwaiter().GetResult();
        _scope.Dispose();
    }
}