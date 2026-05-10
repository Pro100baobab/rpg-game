using System.Collections.Generic;

[System.Serializable]
public class GameData
{
    public PlayerData player;
    public List<EnemyData> enemies;
    public bool isPeacefulMode;
}

[System.Serializable]
public class PlayerData
{
    public float posX, posY, posZ;
    public float health;
    public float remainingCooldown;
}

[System.Serializable]
public class EnemyData
{
    public string enemyType;
    public float posX, posY, posZ;
    public float health;
    // Вариативность
    public int meleeRightWeaponIndex = -1;
    public int meleeLeftWeaponIndex = -1;
    public int evilWatcherAttackType = 0; // 0 = Fireball, 1 = Needle
}