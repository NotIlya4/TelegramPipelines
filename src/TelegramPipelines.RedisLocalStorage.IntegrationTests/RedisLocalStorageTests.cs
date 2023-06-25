using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using StackExchange.Redis.Extensions.Core.Abstractions;
using TelegramPipelines.Abstractions;

namespace TelegramPipelines.UnitTests;

[Collection(nameof(AppFixture))]
public class RedisLocalStorageTests : IDisposable
{
    private readonly IRedisDatabase _redis;
    private readonly IServiceScope _scope;
    private readonly IRedisClientFactory _redisFactory;
    private readonly RedisLocalStorage.RedisRecursiveLocalStorage _recursiveLocalStorage;
    private const string Identity = "test";

    public RedisLocalStorageTests(AppFixture fixture)
    {
        _scope = fixture.Services.CreateScope();
        _redis = _scope.ServiceProvider.GetRequiredService<IRedisDatabase>();
        _redisFactory = _scope.ServiceProvider.GetRequiredService<IRedisClientFactory>();
        _recursiveLocalStorage = RedisLocalStorage.RedisRecursiveLocalStorage.Create(_redisFactory, Identity).GetAwaiter().GetResult();
    }

    [Fact]
    public async Task Save_EmptyRedisStorage_RedisStorageWithValue()
    {
        TestClass expect = new TestClass(10, "asd");
        await _recursiveLocalStorage.Save("asd", expect);
        TestClass? result = (await _redis.GetAsync<JObject>(Identity))?["asd"]?.ToObject<TestClass>();

        Assert.Equal(expect, result);
    }

    [Fact]
    public async Task Get_RedisStorageWithValue_ReturnThatValue()
    {
        TestClass expect = new TestClass(10, "asd");
        await _recursiveLocalStorage.Save("asd", expect);
        TestClass? result = await _recursiveLocalStorage.Get<TestClass>("asd");

        Assert.Equal(expect, result);
    }

    [Fact]
    public async Task Remove_RedisWithValue_RemoveThatValue()
    {
        await _recursiveLocalStorage.Save("asd", new TestClass(10, "asd"));
        await _recursiveLocalStorage.Remove("asd");
        TestClass? result = await _recursiveLocalStorage.Get<TestClass>("asd");

        Assert.Null(result);
    }

    [Fact]
    public async Task Create_EmptyRedis_AfterCreateInStorageIdentityKeyKeepValue()
    {
        var expect = new JObject()
        {
            ["__keep"] = "keep"
        };
        await RedisLocalStorage.RedisRecursiveLocalStorage.Create(_redisFactory, "asd");
        JObject? result = await _redis.GetAsync<JObject>("asd");

        Assert.Equal(expect, result);
    }

    [Fact]
    public async Task CreateChild_EmptyStorage_AddChildrenPropertyInStorage()
    {
        var expect = new List<string>() { "a", "b" };
        await _recursiveLocalStorage.GetOrCreateChild("a");
        await _recursiveLocalStorage.GetOrCreateChild("b");

        var result = (await _redis.GetAsync<JObject>(Identity))?["__children"]?.ToObject<List<string>>();

        Assert.Equal(expect, result);
    }

    [Fact]
    public async Task RemoveStorageAndAllItsChildren_StorageGraph_RemoveWholeGraph()
    {
        IRecursiveLocalStorage child1 = await _recursiveLocalStorage.GetOrCreateChild("child1");
        IRecursiveLocalStorage child2 = await _recursiveLocalStorage.GetOrCreateChild("child2");
        await child1.GetOrCreateChild("child1child1");
        await child1.GetOrCreateChild("child1child2");

        await _recursiveLocalStorage.RemoveStorageAndAllItsChildren();
        
        Assert.False(await _redis.ExistsAsync(Identity));
        Assert.False(await _redis.ExistsAsync("child1"));
        Assert.False(await _redis.ExistsAsync("child2"));
        Assert.False(await _redis.ExistsAsync("child1child1"));
        Assert.False(await _redis.ExistsAsync("child1child2"));
    }

    public void Dispose()
    {
        _redis.FlushDbAsync().GetAwaiter().GetResult();
        _scope.Dispose();
    }
}