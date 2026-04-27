using UnityEngine;
using UnityEngine.AI;

public class EvilWatcherDeathHandler : MonoBehaviour
{
    private IHealth health;
    private EvilWatcher evilWatcher;

    private void Awake()
    {
        health = GetComponent<IHealth>();
        evilWatcher = GetComponent<EvilWatcher>();
    }

    private void OnEnable()
    {
        if (health != null)
            health.OnDeath += HandleDeath;

        Invoke("SetUp", 1f);
    }

    private void OnDisable()
    {
        if (health != null)
            health.OnDeath -= HandleDeath;
 
        if (EventSystem.Instance != null)
            EventSystem.Instance.OnRestart -= HandleRestart;
    }

    private void SetUp() => EventSystem.Instance.OnRestart += HandleRestart;

    private void HandleDeath()
    {
        evilWatcher?.HandleDeath();
    }

    private void HandleRestart()
    {
        evilWatcher?.HandleRestart();
    }
}