namespace TelegramPipelines.Abstractions;

public interface IRecursiveLocalStorageFactory
{
    public Task<IRecursiveLocalStorage> GetOrCreateStorage(TelegramPipelineIdentity storageIdentity);
}