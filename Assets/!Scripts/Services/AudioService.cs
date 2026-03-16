using UnityEngine;

public class AudioService : IAudioService
{
    private const string GeneralVolumeKey = "GeneralVolume";
    private const string MusicVolumeKey = "MusicVolume";
    private const string SfxVolumeKey = "SfxVolume";

    private float _generalVolume = 0.5f;
    private float _musicVolume = 0.5f;
    private float _sfxVolume = 0.5f;

    private readonly AudioSource _musicSource;
    private readonly AudioSource _sfxSource;
    public AudioService(AudioSource musicSource, AudioSource sfxSource)
    {
        _musicSource = musicSource;
        _sfxSource = sfxSource;

        LoadSettings();
        ApplyMusicVolume();
        ApplySfxVolume();
    }

    public float GeneralVolume
    {
        get => _generalVolume;
        set
        {
            _generalVolume = Mathf.Clamp01(value);
            ApplyMusicVolume();
            ApplySfxVolume();
        }
    }

    public float MusicVolume
    {
        get => _musicVolume;
        set
        {
            _musicVolume = Mathf.Clamp01(value);
            ApplyMusicVolume();
        }
    }

    public float SfxVolume
    {
        get => _sfxVolume;
        set
        {
            _sfxVolume = Mathf.Clamp01(value);
            ApplySfxVolume();
        }
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetFloat(GeneralVolumeKey, _generalVolume);
        PlayerPrefs.SetFloat(MusicVolumeKey, _musicVolume);
        PlayerPrefs.SetFloat(SfxVolumeKey, _sfxVolume);
        PlayerPrefs.Save();
    }

    public void LoadSettings()
    {
        _generalVolume = PlayerPrefs.GetFloat(GeneralVolumeKey, 0.5f);
        _musicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, 0.5f);
        _sfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, 0.5f);
    }


    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        _musicSource.clip = clip;
        _musicSource.loop = loop;
        _musicSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        _sfxSource.PlayOneShot(clip);
    }

    public void StopMusic()
    {
        _musicSource.Stop();
    }

    private void ApplyMusicVolume() => _musicSource.volume = MusicVolume * GeneralVolume;
    private void ApplySfxVolume() => _sfxSource.volume = SfxVolume * GeneralVolume;
}