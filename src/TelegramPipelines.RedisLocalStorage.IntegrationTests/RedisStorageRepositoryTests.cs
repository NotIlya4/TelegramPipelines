using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using StackExchange.Redis.Extensions.Core.Abstractions;
using TelegramPipelines.RedisLocalStorage;

namespace TelegramPipelines.UnitTests;

[Collection(nameof(AppFixture))]
public class RedisStorageRepositoryTests : IDisposable
{
    private readonly RedisStorageRepository _repository;
    private readonly IRedisDatabase _redis;
    private readonly IServiceScope _scope;
    private readonly JObject _parentComplexExpect = new JObject()
    {
        ["__keep"] = "keep",
        ["__children"] = new JArray() { "b", "c" }
    };

    private readonly JObject _childBComplexExpect = new JObject()
    {
        ["__keep"] = "keep",
        ["__children"] = new JArray() { "d", "e" }
    };
    private readonly JObject _childCComplexExpect = new JObject()
    {
        ["__keep"] = "keep",
        ["__children"] = new JArray() { "f", "g" }
    };

    public RedisStorageRepositoryTests(AppFixture fixture)
    {
        _scope = fixture.Services.CreateScope();
        _redis = _scope.ServiceProvider.GetRequiredService<IRedisDatabase>();
        _repository = new RedisStorageRepository(_redis);
    }

    [Fact]
    public async Task CreateChildStorage_SimpleCaseWhereWeHaveOnly1LayerGraph_ListOfChildren()
    {
        JObject expect = new JObject()
        {
            ["__keep"] = "keep",
            ["__children"] = new JArray() { "b", "c", "d" }
        };
        await _repository.CreateChildStorage("a", "b");
        await _repository.CreateChildStorage("a", "c");
        await _repository.CreateChildStorage("a", "d");

        JObject? result = await _redis.GetAsync<JObject>("a");
        
        Assert.Equal(expect, result);
    }

    [Fact]
    public async Task CreateChildStorage_ComplexExampleWithMultiLevelGraph_ListOfChildrenAndThenChildrenHasAlsoListOfChildren()
    {
        await CreateComplexStorageGraph();
        
        JObject? parentResult = await _redis.GetAsync<JObject>("a");
        JObject? childBResult = await _redis.GetAsync<JObject>("b");
        JObject? childCResult = await _redis.GetAsync<JObject>("c");
        
        Assert.Equal(_parentComplexExpect, parentResult);
        Assert.Equal(_childBComplexExpect, childBResult);
        Assert.Equal(_childCComplexExpect, childCResult);
    }

    [Fact]
    public async Task GetStorageChildrenRecursive_ComplexStorageGraph_GetAllItsChildren()
    {
        await CreateComplexStorageGraph();
        HashSet<string> expect = new HashSet<string>() { "b", "c", "d", "e", "f", "g" };

        HashSet<string> result = await _repository.GetStorageChildrenRecursive("a");
        
        Assert.Equal(expect, result);
    }

    [Fact]
    public async Task RemoveStorageAndAllItsChildren_ParentAndChildStorageWithValues_RemovedStorages()
    {
        await (await RedisPrimitiveStorage.Create(_redis, "a")).Save("asd", new TestClass(10, "asd"));
        await _repository.CreateChildStorage("a", "b");
        await (await RedisPrimitiveStorage.Create(_redis, "b")).Save("asd", new TestClass(10, "asd"));

        await _repository.RemoveStorageAndAllItsChildren("a");
        
        Assert.False(await _redis.ExistsAsync("a"));
        Assert.False(await _redis.ExistsAsync("b"));
    }

    private async Task CreateComplexStorageGraph()
    {
        await _repository.CreateChildStorage("a", "b");
        await _repository.CreateChildStorage("a", "c");
        await _repository.CreateChildStorage("b", "d");
        await _repository.CreateChildStorage("b", "e");
        await _repository.CreateChildStorage("c", "f");
        await _repository.CreateChildStorage("c", "g");
    }

    public void Dispose()
    {
        _redis.FlushDbAsync().GetAwaiter().GetResult();
        _scope.Dispose();
    }
}