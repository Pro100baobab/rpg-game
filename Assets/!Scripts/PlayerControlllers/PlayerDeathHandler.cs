using UnityEngine;

public class PlayerDeathHandler : MonoBehaviour
{
    private IHealth health;
    private PlayerController playerController;
    private Animator animator;

    private void Awake()
    {
        health = GetComponent<IHealth>();
        playerController = GetComponent<PlayerController>();
        animator = GetComponent<Animator>();
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
        EventSystem.Instance.OnRestart -= HandleRestart;
    }

    private void SetUp() => EventSystem.Instance.OnRestart += HandleRestart;

    private void HandleDeath()
    {
        if (playerController != null)
            playerController.enabled = false;
        if (animator != null)
            animator.SetTrigger("Die");
        
        EventSystem.Instance.GameOver();
    }

    private void HandleRestart()
    {
        if (playerController != null)
            playerController.enabled = true;
        // «доровье восстановитс€ автоматически через Health.ResetHealth
    }
}