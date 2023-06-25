using TelegramPipelines.Abstractions;
using TelegramPipelines.LocalStorage;

namespace TelegramPipelines.NestedPipeline;

public class PipelineLocalStorage : ITelegramPipelineLocalStorage
{
    private readonly IRecursiveLocalStorage _localStorage;
    private readonly ITelegramPipelineIdentitySerializer _identitySerializer;

    public PipelineLocalStorage(IRecursiveLocalStorage localStorage, ITelegramPipelineIdentitySerializer identitySerializer)
    {
        _localStorage = localStorage;
        _identitySerializer = identitySerializer;
    }

    public async Task Save<T>(string key, T o) where T : class
    {
        await _localStorage.Save(key, o);
    }

    public async Task<T?> Get<T>(string key) where T : class
    {
        return await _localStorage.Get<T>(key);
    }

    public async Task Remove(string key)
    {
        await _localStorage.Remove(key);
    }

    public async Task<PipelineLocalStorage> CreateChild(TelegramPipelineIdentity childIdentity)
    {
        string childStorageSignature = _identitySerializer.Serialize(childIdentity);
        return new PipelineLocalStorage(await _localStorage.GetOrCreateChild(childStorageSignature), _identitySerializer);
    }

    public async Task RemoveStorageAndAllItsChildren()
    {
        await _localStorage.RemoveStorageAndAllItsChildren();
    }
}