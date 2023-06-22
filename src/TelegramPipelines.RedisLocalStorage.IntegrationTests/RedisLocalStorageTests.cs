﻿using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using StackExchange.Redis.Extensions.Core.Abstractions;
using TelegramPipelines.Abstractions;
using TelegramPipelines.RedisStorageMaster;

namespace TelegramPipelines.UnitTests;

[Collection(nameof(AppFixture))]
public class RedisLocalStorageTests : IDisposable
{
    private readonly IRedisDatabase _redis;
    private readonly IServiceScope _scope;
    private readonly IRedisClientFactory _redisFactory;
    private readonly RedisLocalStorage _localStorage;
    private const string Identity = "test";

    public RedisLocalStorageTests(AppFixture fixture)
    {
        _scope = fixture.Services.CreateScope();
        _redis = _scope.ServiceProvider.GetRequiredService<IRedisDatabase>();
        _redisFactory = _scope.ServiceProvider.GetRequiredService<IRedisClientFactory>();
        _localStorage = RedisLocalStorage.Create(_redisFactory, Identity).GetAwaiter().GetResult();
    }

    [Fact]
    public async Task Save_EmptyRedisStorage_RedisStorageWithValue()
    {
        TestClass expect = new TestClass(10, "asd");
        await _localStorage.Save("asd", expect);
        TestClass? result = (await _redis.GetAsync<JObject>(Identity))?["asd"]?.ToObject<TestClass>();

        Assert.Equal(expect, result);
    }

    [Fact]
    public async Task Get_RedisStorageWithValue_ReturnThatValue()
    {
        TestClass expect = new TestClass(10, "asd");
        await _localStorage.Save("asd", expect);
        TestClass? result = await _localStorage.Get<TestClass>("asd");

        Assert.Equal(expect, result);
    }

    [Fact]
    public async Task Remove_RedisWithValue_RemoveThatValue()
    {
        await _localStorage.Save("asd", new TestClass(10, "asd"));
        await _localStorage.Remove("asd");
        TestClass? result = await _localStorage.Get<TestClass>("asd");

        Assert.Null(result);
    }

    [Fact]
    public async Task Create_EmptyRedis_AfterCreateInStorageIdentityKeyKeepValue()
    {
        var expect = new JObject()
        {
            ["__keep"] = "keep"
        };
        await RedisLocalStorage.Create(_redisFactory, "asd");
        JObject? result = await _redis.GetAsync<JObject>("asd");

        Assert.Equal(expect, result);
    }

    [Fact]
    public async Task CreateChild_EmptyStorage_AddChildrenPropertyInStorage()
    {
        var expect = new List<string>() { "a", "b" };
        await _localStorage.CreateChildren("a");
        await _localStorage.CreateChildren("b");

        var result = (await _redis.GetAsync<JObject>(Identity))?["__children"]?.ToObject<List<string>>();

        Assert.Equal(expect, result);
    }

    [Fact]
    public async Task RemoveStorageAndAllItsChildren_StorageGraph_RemoveWholeGraph()
    {
        ILocalStorage child1 = await _localStorage.CreateChildren("child1");
        ILocalStorage child2 = await _localStorage.CreateChildren("child2");
        await child1.CreateChildren("child1child1");
        await child1.CreateChildren("child1child2");

        await _localStorage.RemoveStorageAndAllItsChildren();
        
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