using UnityEngine;

public class BossIdleState : EnemyState
{
    private float idleTimer;
    private float patrolTimeout = 5f;

    public BossIdleState(EnemyStateMachine sm) : base(sm) { }

    public override void Enter()
    {
        Context.Animator.SetFloat("Speed", 0f);
        if (Context.Agent != null)
            Context.Agent.isStopped = true;

        idleTimer = 0f;
    }

    public override void Update()
    {
        if (Context.Health.CurrentHealth <= 0) return;

        idleTimer += Time.deltaTime;

        if (!Context.IsPeacefulMode && PlayerInDetectionRange())
        {
            StateMachine.ChangeState(new BossAggressiveState(StateMachine));
        }
        else if (idleTimer >= patrolTimeout && Context.PatrolPoints != null && Context.PatrolPoints.Length > 0)
        {
            StateMachine.ChangeState(new PatrolState(StateMachine));
        }
    }

    public override void OnDamaged()
    {
        StateMachine.ChangeState(new BossAggressiveState(StateMachine));
    }

    private bool PlayerInDetectionRange()
    {
        if (Context.PlayerTransform == null) return false;
        float dist = Vector3.Distance(Context.Transform.position, Context.PlayerTransform.position);
        return dist <= Context.Settings.DetectionRange;
    }
}