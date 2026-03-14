using UnityEngine;

public class DamageAnimationHandler : MonoBehaviour
{
    private IHealth health;
    private Animator animator;

    private void Awake()
    {
        health = GetComponent<IHealth>();
        animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        if (health != null)
            health.OnTakeDamage += HandleTakeDamage;
    }

    private void OnDisable()
    {
        if (health != null)
            health.OnTakeDamage -= HandleTakeDamage;
    }

    private void HandleTakeDamage(DamageType damageType)
    {
        if (animator == null) return;

        int chance = Random.Range(0, 3);
        if (chance == 0 || damageType == DamageType.Magic )
        {
            animator.SetTrigger("GetHit");
        }
    }
}