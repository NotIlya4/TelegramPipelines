namespace Core.TelegramFramework.StatefulPipeline;

public interface IPipelineLocalStorageMaster
{
    Task Clear();
    IPipelineLocalStorage ToPipelineLocalStorage();
    IPipelineLocalStorageMaster CreateChild(string childPrefix);
}

public interface IPipelineLocalStorage
{
    Task Save<T>(string key, T o);
    Task<T> Get<T>(string key);
    Task Remove(string key);
}