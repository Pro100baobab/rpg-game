public enum DamageType
{
    Physical,
    Magic
}

public interface IHealth
{
    float CurrentHealth { get; }
    float MaxHealth { get; }
    void TakeDamage(float amount, DamageType type);

    event System.Action<float, float> OnHealthChanged; // текущее, максимальное
    event System.Action<DamageType> OnTakeDamage;
    event System.Action OnDeath;
}