using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using StackExchange.Redis.Extensions.Core.Abstractions;
using TelegramPipelines.Abstractions;
using TelegramPipelines.RedisLocalStorage;

namespace TelegramPipelines.UnitTests;

[Collection(nameof(AppFixture))]
public class RedisStorageRepositoryTests : IDisposable
{
    private readonly RedisChildStorageRepository _repository;
    private readonly RedisPrimitiveStorage _storage;
    private readonly TelegramPipelineIdentity _identity;
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
        _identity = new TelegramPipelineIdentity(100, new[] { "main" });
        _storage = RedisPrimitiveStorage.Create(_redis.Database, _identity.ColonConcat()).GetAwaiter().GetResult();
        _repository = new RedisChildStorageRepository(_storage);
    }

    // [Fact]
    // public async Task CreateChildStorage_SimpleCaseWhereWeHaveOnly1LayerGraph_ListOfChildren()
    // {
    //     JObject expect = new JObject()
    //     {
    //         ["__keep"] = "keep",
    //         ["__children"] = new JArray() { "100:main:b", "100:main:c", "100:main:d" }
    //     };
    //     await _repository.AddChild(_identity.CreateChild("b"));
    //     await _repository.AddChild(_identity.CreateChild("c"));
    //     await _repository.AddChild(_identity.CreateChild("d"));
    //
    //     JObject? result = await _redis.GetAsync<JObject>(_identity.ColonConcat());
    //     
    //     Assert.Equal(expect, result);
    // }
    //
    // [Fact]
    // public async Task CreateChildStorage_ComplexExampleWithMultiLevelGraph_ListOfChildrenAndThenChildrenHasAlsoListOfChildren()
    // {
    //     await CreateComplexStorageGraph();
    //     
    //     JObject? parentResult = await _redis.GetAsync<JObject>("a");
    //     JObject? childBResult = await _redis.GetAsync<JObject>("b");
    //     JObject? childCResult = await _redis.GetAsync<JObject>("c");
    //     
    //     Assert.Equal(_parentComplexExpect, parentResult);
    //     Assert.Equal(_childBComplexExpect, childBResult);
    //     Assert.Equal(_childCComplexExpect, childCResult);
    // }
    //
    // [Fact]
    // public async Task GetStorageChildrenRecursive_ComplexStorageGraph_GetAllItsChildren()
    // {
    //     await CreateComplexStorageGraph();
    //     HashSet<string> expect = new HashSet<string>() { "b", "c", "d", "e", "f", "g" };
    //
    //     HashSet<string> result = await _repository.GetChildrenRecursive("a");
    //     
    //     Assert.Equal(expect, result);
    // }
    //
    // [Fact]
    // public async Task RemoveStorageAndAllItsChildren_ParentAndChildStorageWithValues_RemovedStorages()
    // {
    //     await (await RedisPrimitiveStorage.Create(_redis, "a")).Save("asd", new TestClass(10, "asd"));
    //     await _repository.AddChild("a", "b");
    //     await (await RedisPrimitiveStorage.Create(_redis, "b")).Save("asd", new TestClass(10, "asd"));
    //
    //     await _repository.ClearStorageRecursive("a");
    //     
    //     Assert.False(await _redis.ExistsAsync("a"));
    //     Assert.False(await _redis.ExistsAsync("b"));
    // }
    //
    // private async Task CreateComplexStorageGraph()
    // {
    //     await _repository.AddChild("b");
    //     await _repository.AddChild("c");
    //     await _repository.AddChild("d");
    //     await _repository.AddChild("e");
    //     await _repository.AddChild("a", "c");
    //     await _repository.AddChild("b", "d");
    //     await _repository.AddChild("b", "e");
    //     await _repository.AddChild("c", "f");
    //     await _repository.AddChild("c", "g");
    // }

    public void Dispose()
    {
        _redis.FlushDbAsync().GetAwaiter().GetResult();
        _scope.Dispose();
    }
}