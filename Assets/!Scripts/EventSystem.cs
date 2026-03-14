using System;
using UnityEngine;

public class EventSystem : MonoBehaviour
{
    public static EventSystem Instance { get; private set; }

    public event Action OnRestart;
    public event Action onGameOver;

    public event Action<int> onAbilityCooldown;
    public event Action resetAbilityCooldown;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        Restart();
    }

    public void Restart()
    {
        OnRestart?.Invoke();
        ResetAbilityCooldown();
    }
    public void GameOver() => onGameOver?.Invoke();

    public void AbilityCooldown(int seconds) => onAbilityCooldown?.Invoke(seconds); // для активации кулдауна
    public void ResetAbilityCooldown() => resetAbilityCooldown?.Invoke(); // для сброса кулдауна
}