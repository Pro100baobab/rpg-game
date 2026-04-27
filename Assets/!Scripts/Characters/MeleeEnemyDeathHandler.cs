using UnityEngine;
using UnityEngine.AI;

public class MeleeEnemyDeathHandler : MonoBehaviour
{
    private IHealth health;
    private MeleeEnemy meleeEnemy;


    private void Awake()
    {
        health = GetComponent<IHealth>();
        meleeEnemy = GetComponent<MeleeEnemy>();
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
        meleeEnemy?.HandleDeath();
    }

    private void HandleRestart()
    {
        meleeEnemy?.HandleRestart();
    }
}