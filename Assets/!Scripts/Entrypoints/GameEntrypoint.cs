using UnityEngine;

public class GameEntrypoint : MonoBehaviour
{
    public static GameEntrypoint Instance { get; private set; }

    [SerializeField] private VoidEventChannelSO victoryMusicEvent;
    [SerializeField] private AudioClip victoryClip;

    public IAudioService AudioService { get; private set; }
    public ISaveService SaveService { get; private set; }
    public ISceneLoader SceneLoader { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeServices();
        LoadStartScene();
    }

    private void InitializeServices()
    {
        AudioService = SetUpAudioService();
        SaveService = new SaveService();
        SceneLoader = new SceneLoader();
    }

    private AudioService SetUpAudioService()
    {
        // Создаём объект для аудио
        var audioGO = new GameObject("Audio");
        audioGO.transform.SetParent(transform);

        var musicSource = audioGO.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;

        var sfxSource = audioGO.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;

        return new AudioService(musicSource, sfxSource);
    }

    private void LoadStartScene() => SceneLoader.LoadScene("MainMenu");

    // Логика победной музыки
    private void Start()
    {
        victoryMusicEvent.OnEventRaised += PlayMusic;
    }

    private void OnDestroy()
    {
        victoryMusicEvent.OnEventRaised -= PlayMusic;
    }

    private void PlayMusic()
    {
        if (AudioService != null && victoryClip != null)
            AudioService.PlayMusic(victoryClip);
    }
}