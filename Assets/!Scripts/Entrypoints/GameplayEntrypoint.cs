using System.Collections.Generic;
using UnityEngine;

public class GameplayEntrypoint : MonoBehaviour
{
    [SerializeField] private GameplayView gameplayView;
    [SerializeField] private GameObject player;

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


        List<GameObject> enemyObjects = new List<GameObject>();

        var spawner = FindAnyObjectByType<SpawnController>();
        if (spawner != null)
            enemyObjects.AddRange(spawner.Enemies);

        _model = new GameModel
        {
            Player = player,
            Enemies = new List<GameObject>(enemyObjects)
        };


        var repository = new GameRepository(saveService);
        var interactor = new GameInteractor(repository, _model);

        _controller = new GameplayController(gameplayView, audioService, interactor, sceneLoader, buttonClickClip, buttonHoverClip);
    }

    private void OnDestroy() => _controller?.Dispose();
}