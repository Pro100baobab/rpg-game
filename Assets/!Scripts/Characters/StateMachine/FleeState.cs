using UnityEngine;

public class FleeState : EnemyState
{
    public FleeState(EnemyStateMachine sm) : base(sm) { }

    public override void Enter()
    {
        Context.Agent.isStopped = false;
        Context.Animator.SetFloat("Speed", 1f);
    }

    public override void Update()
    {
        if (Context.PlayerTransform == null) return;

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