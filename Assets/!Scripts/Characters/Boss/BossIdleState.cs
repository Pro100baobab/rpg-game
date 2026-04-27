using UnityEngine;

public class BossIdleState : EnemyState
{
    public BossIdleState(EnemyStateMachine sm) : base(sm) { }

    public override void Enter()
    {
        Context.Animator.SetFloat("Speed", 0f);
        if (Context.Agent != null)
            Context.Agent.isStopped = true;
    }

    public override void Update()
    {
        if (Context.Health.CurrentHealth <= 0) return;

        if (!Context.IsPeacefulMode && PlayerInDetectionRange())
        {
            StateMachine.ChangeState(new BossAggressiveState(StateMachine));
        }
    }

    public override void OnDamaged()
    {
        // Босс всегда агрится при получении урона, даже в мирном режиме
        StateMachine.ChangeState(new BossAggressiveState(StateMachine));
    }

    private bool PlayerInDetectionRange()
    {
        if (Context.PlayerTransform == null) return false;
        float dist = Vector3.Distance(Context.Transform.position, Context.PlayerTransform.position);
        return dist <= Context.Settings.DetectionRange;
    }
}