using System.Collections.Generic;
using UnityEngine;

public class GameplayEntrypoint : MonoBehaviour
{
    [SerializeField] private GameplayView gameplayView;
    [SerializeField] private GameObject player;
    [SerializeField] private SpawnManager spawnManager;

    [Header("SoundReferences")]
    [SerializeField] private AudioClip[] musicClip;
    [SerializeField] private AudioClip buttonHoverClip;
    [SerializeField] private AudioClip buttonClickClip;

    private GameplayController _controller;
    private GameModel _model;

    private void Start()
    {
        var saveService = GameEntrypoint.Instance.SaveService;
        var sceneLoader = GameEntrypoint.Instance.SceneLoader;

        var audioService = GameEntrypoint.Instance.AudioService;

        if (musicClip != null)
        {
            int clipNumber = Random.Range(0, musicClip.Length);
            audioService.PlayMusic(musicClip[clipNumber]);
        }

        // Создаём модель с пустым списком врагов (они появятся только по кнопке)
        _model = new GameModel
        {
            Player = player,
            Enemies = new List<GameObject>(),
            IsPeacefulMode = true
        };

        var repository = new GameRepository(saveService);
        var saveInteractor = new GameSaveInteractor(repository, _model);
        var loadInteractor = new GameLoadInteractor(repository, _model, spawnManager);

        _controller = new GameplayController(gameplayView, audioService, saveInteractor, loadInteractor, sceneLoader, buttonClickClip, buttonHoverClip, _model);
    }

    private void OnDestroy() => _controller?.Dispose();
}