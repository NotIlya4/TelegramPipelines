namespace TelegramPipelines.Abstractions;

public interface IRecursiveLocalStorageFactory
{
    public Task<IRecursiveLocalStorage> Create(string storageIdentity);
}