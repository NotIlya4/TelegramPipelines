namespace TelegramPipelines.Abstractions;

public interface ILocalStorage
{
    Task Save<T>(string key, T o) where T : class;
    Task<T?> Get<T>(string key) where T : class;
    Task Remove(string key);
    Task RemoveStorageAndAllItsChildren();
    Task<ILocalStorage> CreateChildren(string childStorageIdentifier);
}