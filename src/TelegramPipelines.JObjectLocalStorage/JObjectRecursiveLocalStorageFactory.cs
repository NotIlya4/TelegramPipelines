using Newtonsoft.Json.Linq;
using TelegramPipelines.Abstractions;

namespace TelegramPipelines.JObjectLocalStorage;

public class JObjectRecursiveLocalStorageFactory : IRecursiveLocalStorageFactory
{
    private readonly JObject _storageRoot;

    public JObjectRecursiveLocalStorageFactory(JObject storageRoot)
    {
        _storageRoot = storageRoot;
    }
    
    public Task<IRecursiveLocalStorage> GetOrCreateStorage(TelegramPipelineIdentity storageIdentity)
    {
        var storage = new JObjectRecursiveLocalStorage(_storageRoot, storageIdentity);
        storage.Save("__keep", "keep");
        return Task.FromResult<IRecursiveLocalStorage>(storage);
    }
}