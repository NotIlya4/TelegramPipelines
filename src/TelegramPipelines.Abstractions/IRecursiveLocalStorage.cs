namespace TelegramPipelines.Abstractions;

public interface IRecursiveLocalStorage : ITelegramPipelineLocalStorage
{
    Task RemoveStorageAndAllItsChildren();
    Task<IRecursiveLocalStorage> GetOrCreateChild(string childStorageIdentifier);
}