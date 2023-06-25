using Newtonsoft.Json.Linq;
using TelegramPipelines.Abstractions;

namespace TelegramPipelines.JObjectLocalStorage;

public class JObjectRecursiveLocalStorage : IRecursiveLocalStorage
{
    private readonly JObjectPrimitiveStorage _primitiveStorage;
    public TelegramPipelineIdentity StorageIdentity { get; }
    private const string ChildrenKey = "__children";

    public JObjectRecursiveLocalStorage(JObject storageRoot, TelegramPipelineIdentity storageIdentity)
    {
        _primitiveStorage = new JObjectPrimitiveStorage(storageIdentity.ColonConcat(), storageRoot);
        StorageIdentity = storageIdentity;
    }

    public Task Save<T>(string key, T o) where T : class
    {
        _primitiveStorage.Save(key, o);
        return Task.CompletedTask;
    }

    public Task<T?> Get<T>(string key) where T : class
    {
        return Task.FromResult(_primitiveStorage.Get<T>(key));
    }

    public Task Remove(string key)
    {
        _primitiveStorage.Remove(key);
        return Task.CompletedTask;
    }

    public Task ClearStorageAndAllItsChildren()
    {
        JObject root = _primitiveStorage.StorageRoot;
        HashSet<string> storagesToBeCleared = GetChildrenRecursive();
        storagesToBeCleared.Add(StorageIdentity.ColonConcat());
        foreach (var storage in storagesToBeCleared)
        {
            root.Remove(storage);
        }
        
        return Task.CompletedTask;
    }

    public Task AddChildStorage(IRecursiveLocalStorage newChildStorage)
    {
        HashSet<string> children = GetChildren();
        children.Add(newChildStorage.StorageIdentity.ColonConcat());
        SaveChildren(children);
        return Task.CompletedTask;
    }

    public HashSet<string> GetChildren()
    {
        return _primitiveStorage.Get<HashSet<string>>(ChildrenKey) ?? new HashSet<string>();
    }

    public HashSet<string> GetChildrenRecursive()
    {
        HashSet<string> allChildren = new HashSet<string>() { _primitiveStorage.StorageIdentity };
        Stack<string> toBeChecked = new Stack<string>();
        toBeChecked.Push(_primitiveStorage.StorageIdentity);

        while (toBeChecked.Count > 0)
        {
            var currentStorage = new JObjectPrimitiveStorage(toBeChecked.Pop(), _primitiveStorage.StorageRoot);
            HashSet<string> currentChildren = currentStorage.Get<HashSet<string>>(ChildrenKey) ?? new HashSet<string>();
            foreach (string currentChild in currentChildren)
            {
                if (allChildren.Add(currentChild))
                {
                    toBeChecked.Push(currentChild);
                }
            }
        }

        allChildren.Remove(_primitiveStorage.StorageIdentity);
        return allChildren;
    }

    public void SaveChildren(HashSet<string> children)
    {
        _primitiveStorage.Save(ChildrenKey, children);
    }
}