public class ScoreManager
{
    private readonly IntEventChannelSO scoreChangedEvent;
    private readonly EnemyKilledEventChannelSO enemyKilledEvent;
    public int Score { get; private set; }

    public ScoreManager(IntEventChannelSO scoreChangedEvent, EnemyKilledEventChannelSO enemyKilledEvent)
    {
        this.scoreChangedEvent = scoreChangedEvent;
        this.enemyKilledEvent = enemyKilledEvent;
        enemyKilledEvent.OnEventRaised += OnEnemyKilled;
    }

    private void OnEnemyKilled(KillData data)
    {
        Score += data.ScoreValue;
        scoreChangedEvent.RaiseEvent(Score);
    }

    public void Dispose()
    {
        enemyKilledEvent.OnEventRaised -= OnEnemyKilled;
    }
}