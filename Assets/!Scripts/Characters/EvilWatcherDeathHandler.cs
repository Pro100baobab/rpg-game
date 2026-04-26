using UnityEngine;
using UnityEngine.AI;

public class EvilWatcherDeathHandler : MonoBehaviour
{
    private IHealth health;
    private EvilWatcher evilWatcher;
    private Animator animator;
    private NavMeshAgent agent;

    private void Awake()
    {
        health = GetComponent<IHealth>();
        evilWatcher = GetComponent<EvilWatcher>();
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
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
        if (evilWatcher != null)
            evilWatcher.Die();
        if (agent != null)
            agent.isStopped = true;
        if (animator != null)
            animator.SetTrigger("Die");
    }

    private void HandleRestart()
    {
        if (evilWatcher != null)
            evilWatcher.Revival();
        if (agent != null)
            agent.isStopped = false;
    }
}