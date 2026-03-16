using UnityEngine;
using UnityEngine.UI;
using System;

public class MenuView : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider soundsVolumeSlider;
    [SerializeField] private Slider generalVolumeSlider;
    [SerializeField] private Button closeSettingsButton;

    public event Action OnPlayClicked;
    public event Action<float> OnMusicVolumeChanged;
    public event Action<float> OnSoundsVolumeChanged;
    public event Action<float> OnGeneralVolumeChanged;
    public event Action OnSettingsOpened;
    public event Action OnSettingsClosed;


    public event Action OnPlayButtonHover;
    public event Action OnSettingsButtonHover;
    public event Action OnCloseSettingsButtonHover;

    // Методы для привязки к EventTrigger (PointerEnter)
    public void OnPlayButtonPointerEnter() => OnPlayButtonHover?.Invoke();
    public void OnSettingsButtonPointerEnter() => OnSettingsButtonHover?.Invoke();
    public void OnCloseSettingsButtonPointerEnter() => OnCloseSettingsButtonHover?.Invoke();
    

    private void Awake()
    {
        playButton.onClick.AddListener(() => OnPlayClicked?.Invoke());
        settingsButton.onClick.AddListener(() => OnSettingsOpened?.Invoke());
        musicVolumeSlider.onValueChanged.AddListener(v => OnMusicVolumeChanged?.Invoke(v));
        soundsVolumeSlider.onValueChanged.AddListener(v => OnSoundsVolumeChanged?.Invoke(v));
        generalVolumeSlider.onValueChanged.AddListener(v => OnGeneralVolumeChanged?.Invoke(v));
        closeSettingsButton.onClick.AddListener(() => OnSettingsClosed?.Invoke());
    }

    public void ShowSettingsPanel(bool show) => settingsPanel.SetActive(show);

    public void SetGeneralVolume(float value) => generalVolumeSlider.value = value;
    public void SetMusicVolume(float value) => musicVolumeSlider.value = value;
    public void SetSoundsVolume(float value) => soundsVolumeSlider.value = value;
}