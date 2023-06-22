using StackExchange.Redis.Extensions.Core.Abstractions;

namespace TelegramPipelines.RedisStorageMaster;

public class RedisStorageRepository
{
    private readonly IRedisDatabase _redis;
    private const string StorageChildrenKey = "__children";

    public RedisStorageRepository(IRedisDatabase redis)
    {
        _redis = redis;
    }

    public async Task<HashSet<string>> GetStorageChildrenRecursive(string identity)
    {
        var children = new HashSet<string>() {identity};
        var toBeChecked = new Stack<string>();
        toBeChecked.Push(identity);

        while (toBeChecked.Count > 0)
        {
            HashSet<string> currentStorageChildren = await GetStorageChildren(toBeChecked.Pop());
            foreach (var currentStorageChild in currentStorageChildren)
            {
                if (children.Add(currentStorageChild))
                {
                    toBeChecked.Push(currentStorageChild);
                }
            }
        }
        
        children.Remove(identity);

        return children;
    }

    public async Task CreateChildStorage(string parent, string childIdentity)
    {
        HashSet<string> children = await GetStorageChildren(parent);
        children.Add(childIdentity);
        await SaveStorageChildren(parent, children);
    }

    public async Task RemoveStorageAndAllItsChildren(string identity)
    {
        HashSet<string> children = await GetStorageChildrenRecursive(identity);
        children.Add(identity);

        foreach (var child in children)
        {
            await (await RedisPrimitiveStorage.Create(_redis, child)).DeleteStorage();
        }
    }

    private async Task<HashSet<string>> GetStorageChildren(string identity)
    {
        return await (await RedisPrimitiveStorage.Create(_redis, identity)).Get<HashSet<string>>(StorageChildrenKey) ?? new HashSet<string>();
    }

    private async Task SaveStorageChildren(string identity, HashSet<string> children)
    {
        await (await RedisPrimitiveStorage.Create(_redis, identity)).Save(StorageChildrenKey, children);
    }
}