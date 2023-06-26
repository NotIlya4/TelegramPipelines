using Newtonsoft.Json.Linq;

namespace TelegramPipelines.JObjectLocalStorage;

internal class JObjectPrimitiveStorage
{
    public string StorageIdentity { get; }
    public JObject StorageRoot { get; }

    public JObjectPrimitiveStorage(string storageIdentity, JObject storageRoot)
    {
        StorageIdentity = storageIdentity;
        StorageRoot = storageRoot;

        JObject? storage = StorageRoot[StorageIdentity]?.ToObject<JObject>();
        if (storage is null)
        {
            StorageRoot[StorageIdentity] = new JObject();
        }
    }
    
    public void Save<T>(string key, T o) where T : class
    {
        JObject storage = GetStorage();
        storage[key] = JToken.FromObject(o);
        SaveStorage(storage);
    }

    public T? Get<T>(string key) where T : class
    {
        return GetStorage()[key]?.ToObject<T>();
    }

    public void Remove(string key)
    {
        JObject storage = GetStorage();
        storage.Remove(key);
        SaveStorage(storage);
    }
    
    public JObject GetStorage()
    {
        return StorageRoot[StorageIdentity]?.ToObject<JObject>()!;
    }

    public void SaveStorage(JObject storage)
    {
        StorageRoot[StorageIdentity] = storage;
    }
}