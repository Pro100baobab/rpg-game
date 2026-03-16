using UnityEngine;

public class MenuEntrypoint : MonoBehaviour
{
    [Header("View")]
    [SerializeField] private MenuView menuView;

    [Header("SoundReferences")]
    [SerializeField] private AudioClip[] musicClip;
    [SerializeField] private AudioClip buttonHoverClip;
    [SerializeField] private AudioClip buttonClickClip;

    private MenuController _controller;

    private void Start()
    {
        var audioService = GameEntrypoint.Instance.AudioService;
        var sceneLoader = GameEntrypoint.Instance.SceneLoader;

        if (musicClip != null)
        {
            int clipNumber = Random.Range(0, musicClip.Length);
            audioService.PlayMusic(musicClip[clipNumber]);
        }

        _controller = new MenuController(menuView, audioService, sceneLoader, buttonClickClip, buttonHoverClip);
    }

    private void OnDestroy() => _controller?.Dispose();
}