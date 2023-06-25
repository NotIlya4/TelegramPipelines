namespace TelegramPipelines.Abstractions;

public interface ITelegramPipelineLocalStorage
{
    Task Save<T>(string key, T o) where T : class;
    Task<T?> Get<T>(string key) where T : class;
    Task Remove(string key);
}