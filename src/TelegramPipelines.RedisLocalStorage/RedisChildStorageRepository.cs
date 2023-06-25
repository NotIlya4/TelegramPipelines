using StackExchange.Redis.Extensions.Core.Abstractions;
using TelegramPipelines.Abstractions;

namespace TelegramPipelines.RedisLocalStorage;

internal class RedisChildStorageRepository
{
    private readonly RedisPrimitiveStorage _storage;
    private const string StorageChildrenKey = "__children";

    public RedisChildStorageRepository(RedisPrimitiveStorage storage)
    {
        _storage = storage;
    }

    public async Task<HashSet<string>> GetChildrenRecursive()
    {
        var children = new HashSet<string>() { _storage.StorageIdentity };
        var toBeChecked = new Stack<string>();
        toBeChecked.Push(_storage.StorageIdentity);

        while (toBeChecked.Count > 0)
        {
            var currentStorage = new RedisChildStorageRepository(await RedisPrimitiveStorage.Create(_storage.Redis, toBeChecked.Pop()));
            HashSet<string> currentStorageChildren = await currentStorage.GetChildren();
            foreach (var currentStorageChild in currentStorageChildren)
            {
                if (children.Add(currentStorageChild))
                {
                    toBeChecked.Push(currentStorageChild);
                }
            }
        }
        
        children.Remove(_storage.StorageIdentity);

        return children;
    }

    public async Task AddChild(TelegramPipelineIdentity childStorageIdentity)
    {
        HashSet<string> children = await GetChildren();
        children.Add(childStorageIdentity.ColonConcat());
        await SaveChildren(children);
    }

    public async Task ClearStorageRecursive()
    {
        HashSet<string> children = await GetChildrenRecursive();
        children.Add(_storage.StorageIdentity);

        foreach (var child in children)
        {
            await (await RedisPrimitiveStorage.Create(_storage.Redis, child)).DeleteStorage();
        }
    }

    public async Task<HashSet<string>> GetChildren()
    {
        return await _storage.Get<HashSet<string>>(StorageChildrenKey) ?? new HashSet<string>();
    }

    public async Task SaveChildren(HashSet<string> children)
    {
        await _storage.Save(StorageChildrenKey, children);
    }
}