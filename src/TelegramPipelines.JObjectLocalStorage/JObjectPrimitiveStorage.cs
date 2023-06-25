using Newtonsoft.Json.Linq;

namespace TelegramPipelines.JObjectLocalStorage;

public class JObjectPrimitiveStorage
{
    public string StorageIdentity { get; }
    public JObject StorageRoot { get; }

    public JObjectPrimitiveStorage(string storageIdentity, JObject storageRoot)
    {
        StorageIdentity = storageIdentity;
        StorageRoot = storageRoot;
    }
    
    public void Save<T>(string key, T o) where T : class
    {
        GetStorage()[key] = JToken.FromObject(o);
    }

    public T? Get<T>(string key) where T : class
    {
        return GetStorage()[key]?.ToObject<T>();
    }

    public void Remove(string key)
    {
        GetStorage().Remove(key);
    }
    
    public JObject GetStorage()
    {
        return StorageRoot[StorageIdentity]?.ToObject<JObject>() ?? new JObject();
    }
}