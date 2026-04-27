using UnityEngine;

public class FleeState : EnemyState
{
    private readonly float fleeDistanceMultiplier = 1.35f;

    public FleeState(EnemyStateMachine sm) : base(sm) { }

    public override void Enter()
    {
        Context.Agent.isStopped = false;
        Context.Animator.SetFloat("Speed", 1f);
    }

    public override void Update()
    {
        if (Context.PlayerTransform == null)
        {
            StateMachine.ChangeState(new IdleState(StateMachine));
            return;
        }

        float distance = Vector3.Distance(Context.Transform.position, Context.PlayerTransform.position);
        float safeDistance = Context.Settings.DetectionRange * fleeDistanceMultiplier;

        // Если режим сменился на агрессивный, прекращаем побег и переходим к обычной логике
        if (!Context.IsPeacefulMode)
        {
            if (distance <= Context.Settings.DetectionRange)
                StateMachine.ChangeState(new AggressiveState(StateMachine));
            else
                StateMachine.ChangeState(new IdleState(StateMachine));
            return;
        }

        // В мирном режиме отступаем только до безопасной дистанции
        if (distance >= safeDistance)
        {
            StateMachine.ChangeState(new IdleState(StateMachine));
            return;
        }

        Vector3 dirAway = (Context.Transform.position - Context.PlayerTransform.position).normalized;
        Vector3 fleeTarget = Context.Transform.position + dirAway * 20f;
        Context.Agent.SetDestination(fleeTarget);
        Context.Animator.SetFloat("Speed", Context.Agent.velocity.magnitude);
    }

    public override void Exit()
    {
        Context.Agent.isStopped = true;
    }
}