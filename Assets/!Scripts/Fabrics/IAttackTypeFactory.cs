using UnityEngine;


// Фабричный метод для типа атаки для EvilWatcher
public interface IAttackTypeFactory
{
    EvilWatcherAttackType CreateAttackType();
}

public class RandomAttackTypeFactory : IAttackTypeFactory
{
    public EvilWatcherAttackType CreateAttackType()
    {
        return (EvilWatcherAttackType)Random.Range(0, 2);
    }
}