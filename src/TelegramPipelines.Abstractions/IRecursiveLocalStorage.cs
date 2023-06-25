namespace TelegramPipelines.Abstractions;

public interface IRecursiveLocalStorage : ITelegramPipelineLocalStorage
{
    public TelegramPipelineIdentity StorageIdentity { get; }
    Task ClearStorageAndAllItsChildren();
    Task AddChildStorage(IRecursiveLocalStorage newChildStorage);
}