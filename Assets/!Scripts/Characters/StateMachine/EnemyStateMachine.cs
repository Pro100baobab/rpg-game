public class EnemyStateMachine
{
    public IEnemyContext Context { get; private set; }
    public EnemyState CurrentState { get; private set; }
    public float LastAttackTime { get; set; } = -999f;

    public EnemyStateMachine(IEnemyContext context)
    {
        Context = context;
    }

    public void Initialize(EnemyState initialState)
    {
        Context.Health.OnDeath += HandleDeath;
        Context.Health.OnTakeDamage += HandleTakeDamage;

        if (GameModel.Instance != null)
            GameModel.Instance.OnPeacefulModeChanged += OnPeacefulModeChanged;

        ChangeState(initialState);
    }

    public void ChangeState(EnemyState newState)
    {
        CurrentState?.Exit();
        CurrentState = newState;
        newState.Enter();
    }

    public void Update()
    {
        CurrentState?.Update();
    }

    private void HandleDeath()
    {
        ChangeState(new DeadState(this));
    }

    private void HandleTakeDamage(DamageType type)
    {
        CurrentState?.OnDamaged();
    }

    protected virtual void OnPeacefulModeChanged(bool isPeaceful) { }

    public void Dispose()
    {
        if (Context.Health != null)
        {
            Context.Health.OnDeath -= HandleDeath;
            Context.Health.OnTakeDamage -= HandleTakeDamage;
        }

        if (GameModel.Instance != null)
            GameModel.Instance.OnPeacefulModeChanged -= OnPeacefulModeChanged;
    }
}