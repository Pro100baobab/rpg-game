using UnityEngine;

public interface IAudioService
{
    float GeneralVolume { get; set; }
    float MusicVolume { get; set; }
    float SfxVolume { get; set; }
    void SaveSettings();
    void LoadSettings();


    void PlayMusic(AudioClip clip, bool loop = true);
    void PlaySFX(AudioClip clip);
    void StopMusic();
}

public interface ISaveService
{
    void Save<T>(string key, T data);
    T Load<T>(string key, T defaultValue = default);
    bool HasKey(string key);
}

public interface ISceneLoader
{
    void LoadScene(string sceneName);
    void LoadSceneAsync(string sceneName, System.Action onLoaded = null);
}