using UnityEngine;
using UnityEngine.UI;
using System;

public class GameplayView : MonoBehaviour
{
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button loadButton;
    [SerializeField] private Button mainMenuButton;

    public event Action OnResume;
    public event Action OnSave;
    public event Action OnLoad;
    public event Action OnMainMenu;


    public event Action OnResumeButtonHover;
    public event Action OnSaveButtonHover;
    public event Action OnLoadButtonHover;
    public event Action OnMainMenuButtonHover;

    // Методы для привязки к EventTrigger (PointerEnter)
    public void OnResumeButtonPointerEnter() => OnResumeButtonHover?.Invoke();
    public void OnSaveButtonPointerEnter() => OnSaveButtonHover?.Invoke();
    public void OnLoadButtonPointerEnter() => OnLoadButtonHover?.Invoke();
    public void OnMainMenuButtonPointerEnter() => OnMainMenuButtonHover?.Invoke();


    private void Awake()
    {
        resumeButton.onClick.AddListener(() => OnResume?.Invoke());
        saveButton.onClick.AddListener(() => OnSave?.Invoke());
        loadButton.onClick.AddListener(() => OnLoad?.Invoke());
        mainMenuButton.onClick.AddListener(() => OnMainMenu?.Invoke());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenu();
        }
    }

    public void ToggleMenu()
    {
        bool active = !menuPanel.activeSelf;
        menuPanel.SetActive(active);
        Time.timeScale = active ? 0f : 1f;
    }

    public void ShowMenu(bool show)
    {
        menuPanel.SetActive(show);
        Time.timeScale = show ? 0f : 1f;
    }
}