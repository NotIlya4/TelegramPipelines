using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core.Abstractions;
using TelegramPipelines.RedisStorageMaster;

namespace TelegramPipelines.UnitTests;

[Collection(nameof(AppFixture))]
public class RedisPrimitiveStorageTests : IDisposable
{
    private readonly RedisPrimitiveStorage _storage;
    private readonly IServiceScope _scope;
    private readonly IRedisDatabase _redis;
    private const string Identity = "test-identity";
    
    public RedisPrimitiveStorageTests(AppFixture fixture)
    {
        _scope = fixture.Services.CreateScope();
        _redis = fixture.Services.GetRequiredService<IRedisDatabase>();
        _storage = RedisPrimitiveStorage.Create(_redis, Identity).GetAwaiter().GetResult();
        _redis.FlushDbAsync();
    }

    [Fact]
    public async Task Save_SaveRecord_RedisContainsThatValue()
    {
        var expect = new TestClass(10, "asd");
        
        await _storage.Save("asdasd", expect);
        TestClass? result = (await _redis.GetAsync<JObject>(Identity))?["asdasd"]?.ToObject<TestClass>();
        
        Assert.Equal(expect, result);
    }

    [Fact]
    public async Task Get_RedisContainsValue_ReturnAndParseIt()
    {
        var expect = new TestClass(5, "azx");
        
        var value = new JObject();
        value["aaa"] = JObject.FromObject(expect);
        await _redis.AddAsync(Identity, value);
        TestClass? result = await _storage.Get<TestClass>("aaa");

        Assert.Equal(expect, result);
    }

    [Fact]
    public async Task Remove_RedisContainsValues_RemovesOnlyItAndRemainEverythingElse()
    {
        var expect = new JObject();
        expect["aba"] = JObject.FromObject(new TestClass(133, "asdas"));
        expect["bbb"] = JObject.FromObject(new TestClass(160, "lll"));
        await _redis.AddAsync(Identity, expect);

        await _storage.Remove("aba");
        JObject? result = await _redis.GetAsync<JObject>(Identity);
        expect.Remove("aba");
        
        Assert.Equal(expect, result);
    }

    [Fact]
    public async Task DeleteStorage_DeleteExistingStorage_StorageDeleted()
    {
        await _storage.Save("asd", new TestClass(10, "asd"));
        await _storage.DeleteStorage();

        JObject? result = await _redis.GetAsync<JObject>(Identity);
        
        Assert.Null(result);
    }

    [Fact]
    public async Task Create_EmptyRedis_RedisContainsKeyWithKeep()
    {
        var expect = new JObject()
        {
            ["__keep"] = "keep"
        };
        
        await RedisPrimitiveStorage.Create(_redis, "asd");
        
        Assert.Equal(expect, await _redis.GetAsync<JObject>("asd"));
    }

    public void Dispose()
    {
        _redis.FlushDbAsync().GetAwaiter().GetResult();
        _scope.Dispose();
    }
}

public record TestClass
{
    public TestClass(int biba, string boba)
    {
        Biba = biba;
        Boba = boba;
    }

    public int Biba { get; set; }
    public string Boba { get; set; }
}