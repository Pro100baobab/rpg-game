using UnityEngine;

public class IdleState : EnemyState
{
    public IdleState(EnemyStateMachine sm) : base(sm) { }

    public override void Enter()
    {
        Context.Animator.SetFloat("Speed", 0f);
        Context.Agent.isStopped = true;
    }

    public override void Update()
    {
        if (Context.Health.CurrentHealth <= 0) return;

        // ћирный режим: не агримс€, но убегаем при низком HP
        if (Context.IsPeacefulMode)
        {
            float healthPercent = Context.Health.CurrentHealth / Context.Health.MaxHealth;
            if (healthPercent <= Context.Settings.FleeHealthPercent)
            {
                StateMachine.ChangeState(new FleeState(StateMachine));
            }
            return;
        }

        // ќбычный режим: агримс€ при обнаружении игрока
        if (PlayerInDetectionRange())
        {
            StateMachine.ChangeState(new AggressiveState(StateMachine));
        }
    }

    public override void OnDamaged()
    {
        if (Context.IsPeacefulMode)
        {
            float healthPercent = Context.Health.CurrentHealth / Context.Health.MaxHealth;
            if (healthPercent <= Context.Settings.FleeHealthPercent)
            {
                StateMachine.ChangeState(new FleeState(StateMachine));
            }
            return;
        }

        StateMachine.ChangeState(new AggressiveState(StateMachine));
    }

    private bool PlayerInDetectionRange()
    {
        if (Context.PlayerTransform == null) return false;
        float dist = Vector3.Distance(Context.Transform.position, Context.PlayerTransform.position);
        return dist <= Context.Settings.DetectionRange;
    }
}