using UnityEngine;
using UnityEngine.AI;

public class MeleeEnemyDeathHandler : MonoBehaviour
{
    private IHealth health;
    private MeleeEnemy meleeEnemy;
    private Animator animator;
    private NavMeshAgent agent;

    private void Awake()
    {
        health = GetComponent<IHealth>();
        meleeEnemy = GetComponent<MeleeEnemy>();
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
        if (meleeEnemy != null)
            meleeEnemy.Die();
        if (agent != null)
            agent.isStopped = true;
        if (animator != null)
            animator.SetTrigger("Die");
    }

    private void HandleRestart()
    {
        if (meleeEnemy != null)
            meleeEnemy.Revival();
        if (agent != null)
            agent.isStopped = false;
    }
}