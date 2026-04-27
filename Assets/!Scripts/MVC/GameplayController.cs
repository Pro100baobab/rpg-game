using System;
using UnityEngine;

public class GameplayController : IDisposable
{
    private readonly GameplayView _view;
    private readonly GameSaveInteractor _saveInteractor;
    private readonly GameLoadInteractor _loadInteractor;
    private readonly GameModel _model;

    private readonly IAudioService _audio;
    private readonly ISceneLoader _sceneLoader;

    private readonly AudioClip _buttonClickClip;
    private readonly AudioClip _buttonHoverClip;

    public GameplayController(GameplayView view, IAudioService audio, GameSaveInteractor saveInteractor, GameLoadInteractor loadInteractor, ISceneLoader sceneLoader,
                              AudioClip buttonClickClip, AudioClip buttonHoverClip, GameModel model)
    {
        _view = view;
        _saveInteractor = saveInteractor;
        _loadInteractor = loadInteractor;
        _model = model;
        _audio = audio;
        _sceneLoader = sceneLoader;
        _buttonClickClip = buttonClickClip;
        _buttonHoverClip = buttonHoverClip;

        _view.OnResume += HandleResume;
        _view.OnSave += HandleSave;
        _view.OnLoad += HandleLoad;
        _view.OnMainMenu += HandleMainMenu;
        _view.OnChangeGameMode += HandleGameMode;

        _view.OnResumeButtonHover += () => _audio.PlaySFX(_buttonHoverClip);
        _view.OnSaveButtonHover += () => _audio.PlaySFX(_buttonHoverClip);
        _view.OnLoadButtonHover += () => _audio.PlaySFX(_buttonHoverClip);
        _view.OnMainMenuButtonHover += () => _audio.PlaySFX(_buttonHoverClip);

        _model.OnPeacefulModeChanged += HandlePeacefulModeChanged;
        _view.UpdateGameModeText(_model.IsPeacefulMode);
    }

    private void HandleResume()
    {
        _audio.PlaySFX(_buttonClickClip);
        _view.ShowMenu(false);
    }

    private void HandleSave()
    {
        _saveInteractor.SaveGame();
        _audio.PlaySFX(_buttonClickClip);

        Debug.Log("Игра сохранена");
        _view.ShowMenu(false);
    }

    private void HandleLoad()
    {
        if (_loadInteractor.LoadGame()) Debug.Log("Игра загружена");
        else Debug.LogWarning("Нет сохранения");

        _audio.PlaySFX(_buttonClickClip);
        _view.ShowMenu(false);
    }

    private void HandleMainMenu()
    {
        _audio.PlaySFX(_buttonClickClip);

        Time.timeScale = 1f;
        _sceneLoader.LoadScene("MainMenu");
    }

    private void HandleGameMode()
    {
        if (_model != null)
            _model.IsPeacefulMode = !_model.IsPeacefulMode;
    }

    private void HandlePeacefulModeChanged(bool isPeaceful)
    {
        _view.UpdateGameModeText(isPeaceful);
    }

    public void Dispose()
    {
        _view.OnResume -= HandleResume;
        _view.OnSave -= HandleSave;
        _view.OnLoad -= HandleLoad;
        _view.OnMainMenu -= HandleMainMenu;
        _view.OnChangeGameMode -= HandleGameMode;

        if (_model != null)
            _model.OnPeacefulModeChanged -= HandlePeacefulModeChanged;
    }
}