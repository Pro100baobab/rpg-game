using UnityEngine;

public class BossRetreatState : EnemyState
{
    private float retreatTimer = 2f;
    private float timer;

    public BossRetreatState(EnemyStateMachine sm) : base(sm) { }

    public override void Enter()
    {
        Context.Agent.isStopped = false;
        timer = 0f;
        // Бежим от игрока
        Vector3 dirAway = (Context.Transform.position - Context.PlayerTransform.position).normalized;
        Context.Agent.SetDestination(Context.Transform.position + dirAway * 10f);
        Context.Animator.SetFloat("Speed", 1f);
    }

    public override void Update()
    {
        timer += Time.deltaTime;
        Context.Animator.SetFloat("Speed", Context.Agent.velocity.magnitude);

        if (timer >= retreatTimer)
        {
            StateMachine.ChangeState(new BossAggressiveState(StateMachine));
        }
    }

    public override void Exit()
    {
        Context.Agent.isStopped = true;
    }
}