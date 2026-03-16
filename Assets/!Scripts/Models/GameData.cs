using System.Collections.Generic;

[System.Serializable]
public class GameData
{
    public PlayerData player;
    public List<EnemyData> enemies;
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
}