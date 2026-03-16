using System;
using UnityEngine;

public class MenuController : IDisposable
{
    private readonly MenuView _view;
    private readonly IAudioService _audio;
    private readonly ISceneLoader _sceneLoader;

    private readonly AudioClip _buttonClickClip;
    private readonly AudioClip _buttonHoverClip;

    public MenuController(MenuView view, IAudioService audio, ISceneLoader sceneLoader, 
        AudioClip buttonClickClip, AudioClip buttonHoverClip)
    {
        _view = view;
        _audio = audio;
        _sceneLoader = sceneLoader;
        _buttonClickClip = buttonClickClip;
        _buttonHoverClip = buttonHoverClip;


        _view.OnPlayClicked += HandlePlay;
        _view.OnSettingsOpened += HandleSettingsOpened;
        _view.OnSettingsClosed += HandleSettingsClosed;
        _view.OnGeneralVolumeChanged += HandleGeneralVolumeChanged;
        _view.OnMusicVolumeChanged += HandleMusicVolumeChanged;
        _view.OnSoundsVolumeChanged += HandleSoundsVolumeChanged;

        _view.OnPlayButtonHover += () => _audio.PlaySFX(_buttonHoverClip);
        _view.OnSettingsButtonHover += () => _audio.PlaySFX(_buttonHoverClip);
        _view.OnCloseSettingsButtonHover += () => _audio.PlaySFX(_buttonHoverClip);

        _view.SetGeneralVolume(_audio.GeneralVolume);
        _view.SetMusicVolume(_audio.MusicVolume);
        _view.SetSoundsVolume(_audio.SfxVolume);
        _view.ShowSettingsPanel(false);
    }

    private void HandlePlay()
    {
        _audio.PlaySFX(_buttonClickClip);
        _sceneLoader.LoadScene("Gameplay");
    }

    private void HandleSettingsOpened()
    {
        _audio.PlaySFX(_buttonClickClip);
        _view.ShowSettingsPanel(true);
    }

    private void HandleSettingsClosed()
    {
        _audio.PlaySFX(_buttonClickClip);
        _view.ShowSettingsPanel(false);
        _audio.SaveSettings();
    }

    private void HandleGeneralVolumeChanged(float volume) => _audio.GeneralVolume = volume;
    private void HandleMusicVolumeChanged(float volume) => _audio.MusicVolume = volume;
    private void HandleSoundsVolumeChanged(float volume) => _audio.SfxVolume = volume;

    public void Dispose()
    {
        _view.OnPlayClicked -= HandlePlay;
        _view.OnSettingsOpened -= HandleSettingsOpened;
        _view.OnSettingsClosed -= HandleSettingsClosed;
        _view.OnGeneralVolumeChanged -= HandleGeneralVolumeChanged;
        _view.OnMusicVolumeChanged -= HandleMusicVolumeChanged;
        _view.OnSoundsVolumeChanged -= HandleSoundsVolumeChanged;
    }
}