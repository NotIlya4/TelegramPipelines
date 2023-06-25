using Newtonsoft.Json.Linq;
using TelegramPipelines.Abstractions;

namespace TelegramPipelines.JObjectLocalStorage.Tests;

public class JObjectRecursiveLocalStorageTests
{
    private readonly JObject _root = new();
    private readonly JObjectRecursiveLocalStorageFactory _factory;
    private readonly JObjectRecursiveLocalStorage _storage;
    private readonly TelegramPipelineIdentity _identity;

    public JObjectRecursiveLocalStorageTests()
    {
        _identity = new TelegramPipelineIdentity(100, new[] { "main" });
        _factory = new JObjectRecursiveLocalStorageFactory(_root);
        _storage = (_factory.GetOrCreateStorage(_identity).GetAwaiter().GetResult() as JObjectRecursiveLocalStorage)!;
    }
    
    [Fact]
    public void Create_EmptyJObject_InStorageAppearKeepProperty()
    {
        Assert.Equal("keep", _root[_identity.ColonConcat()]?["__keep"]?.ToObject<string>());
    }

    [Fact]
    public async Task SaveGetRemoveTest()
    {
        await _storage.Save("a", new TestClass(10, "asd"));
        var result = await _storage.Get<TestClass>("a");
        Assert.Equal(new TestClass(10, "asd"), result);
        await _storage.Remove("a");
        var result2 = await _storage.Get<TestClass>("a");
        Assert.Null(result2);
    }

    [Fact]
    public async Task AddChildStorage_StorageWithoutAnyChildren_ChildrenPropertyHasAddedChild()
    {
        var childIdentity = _identity.CreateChild("a");
        IRecursiveLocalStorage child = await _factory.GetOrCreateStorage(childIdentity);
        await _storage.AddChildStorage(child);

        var result = _root[_identity.ColonConcat()]?["__children"]?.ToObject<HashSet<string>>();

        Assert.True(result?.Contains("100:main:a"));
    }

    [Fact]
    public async Task ClearStorageAndAllChildren_StorageWithSomeChildren_StoragesRemoved()
    {
        var child1Identity = _identity.CreateChild("a");
        IRecursiveLocalStorage child1 = await _factory.GetOrCreateStorage(child1Identity);
        await _storage.AddChildStorage(child1);
        var child2Identity = _identity.CreateChild("b");
        IRecursiveLocalStorage child2 = await _factory.GetOrCreateStorage(child2Identity);
        await _storage.AddChildStorage(child2);
        
        await _storage.ClearStorageAndAllItsChildren();
        
        Assert.Null(_root[_identity.ColonConcat()]?["__keep"]);
        Assert.Null(_root[child1Identity.ColonConcat()]?["__keep"]);
        Assert.Null(_root[child2Identity.ColonConcat()]?["__keep"]);
    }
}

public record TestClass(int Biba, string Boba);