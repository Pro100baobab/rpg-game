using UnityEngine;

public class Health : MonoBehaviour, IHealth
{
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    public event System.Action<float, float> OnHealthChanged;
    public event System.Action<DamageType> OnTakeDamage;
    public event System.Action OnDeath;

    private bool isDead = false;

    private void Awake() => currentHealth = maxHealth;

    private void OnEnable() => Invoke("SetUp", 1f);

    private void OnDisable() => EventSystem.Instance.OnRestart -= ResetHealth;

    private void SetUp()
    {
        EventSystem.Instance.OnRestart += ResetHealth;
        Debug.Log("Health.cs подписался на EventSystem");
    }

    public void TakeDamage(float amount, DamageType type)
    {
        if (isDead) return;

        currentHealth -= amount;
        if(currentHealth > 0)
            OnTakeDamage?.Invoke(type);
        
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0f) {
            Die();
            return;
        }
    }


    private void Die()
    {
        if (isDead) return;
        isDead = true;

        OnDeath?.Invoke();
    }

    public void ResetHealth()
    {
        isDead = false;
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
}