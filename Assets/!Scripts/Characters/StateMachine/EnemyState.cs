public abstract class EnemyState
{
    protected EnemyStateMachine StateMachine;
    protected IEnemyContext Context => StateMachine.Context;

    public EnemyState(EnemyStateMachine stateMachine)
    {
        StateMachine = stateMachine;
    }

    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void Update() { }
    public virtual void OnDamaged() { }
}