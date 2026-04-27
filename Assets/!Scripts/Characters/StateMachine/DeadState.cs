using UnityEngine;

public class DeadState : EnemyState
{
    public DeadState(EnemyStateMachine sm) : base(sm) { }

    public override void Enter()
    {
        Context.Agent.isStopped = true;
        Context.Animator.SetTrigger("Die");
    }

    public override void Update() { }
}