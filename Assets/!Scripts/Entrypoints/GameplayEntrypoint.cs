using UnityEngine;

public class GameplayEntrypoint : MonoBehaviour
{
    [Header("View")]
    [SerializeField] private GameplayView gameplayView;

    [Header("SoundReferences")]
    [SerializeField] private AudioClip[] musicClip;
    [SerializeField] private AudioClip buttonHoverClip;
    [SerializeField] private AudioClip buttonClickClip;

    private GameplayController _controller;

    private void Start()
    {
        var saveService = GameEntrypoint.Instance.SaveService;
        var sceneLoader = GameEntrypoint.Instance.SceneLoader;

        var audioService = GameEntrypoint.Instance.AudioService;
        var save = GameEntrypoint.Instance.SaveService;

        if (musicClip != null)
        {
            int clipNumber = Random.Range(0, musicClip.Length);
            audioService.PlayMusic(musicClip[clipNumber]);
        }

        _controller = new GameplayController(gameplayView, audioService, save, sceneLoader, buttonClickClip, buttonHoverClip);
    }

    private void OnDestroy() => _controller?.Dispose();
}