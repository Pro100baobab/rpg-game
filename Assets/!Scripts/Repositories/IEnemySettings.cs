public interface IEnemySettings
{
    float DetectionRange { get; }
    float AttackRange { get; }
    int PhysicalDamage { get; set; }
    float IdealCombatDistance { get; }
    float AttackCooldown { get; }
    float AttackDuration { get; }
    float RotationSpeed { get; }
    float FleeHealthPercent { get; }
}