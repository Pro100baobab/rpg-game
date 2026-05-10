public class KillManager
{
    private readonly EnemyKilledEventChannelSO enemyKilledEvent;
    private readonly VoidEventChannelSO onBossSpawnEvent;
    private readonly VoidEventChannelSO onVictoryMusicEvent;
    private int killCount;

    public KillManager(EnemyKilledEventChannelSO enemyKilledEvent, VoidEventChannelSO onBossSpawnEvent, VoidEventChannelSO onVictoryMusicEvent)
    {
        this.enemyKilledEvent = enemyKilledEvent;
        this.onBossSpawnEvent = onBossSpawnEvent;
        this.onVictoryMusicEvent = onVictoryMusicEvent;

        enemyKilledEvent.OnEventRaised += OnEnemyKilled;
    }

    private void OnEnemyKilled(KillData data)
    {
        if (data.IsBoss) return; // не считаем босса в счётчике обычных убийств

        killCount++;
        if (killCount == 3)
            onBossSpawnEvent.RaiseEvent();
        if (killCount == 5)
            onVictoryMusicEvent.RaiseEvent();
    }

    public void Dispose()
    {
        enemyKilledEvent.OnEventRaised -= OnEnemyKilled;
    }
}