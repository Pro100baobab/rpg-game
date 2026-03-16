using UnityEngine;
using System.IO;

public class SaveService : ISaveService
{
    private string GetPath(string key) => Path.Combine(Application.persistentDataPath, key + ".json");

    public void Save<T>(string key, T data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetPath(key), json);
    }

    public T Load<T>(string key, T defaultValue = default)
    {
        string path = GetPath(key);
        if (!File.Exists(path)) return defaultValue;

        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<T>(json);
    }

    public bool HasKey(string key) => File.Exists(GetPath(key));
}