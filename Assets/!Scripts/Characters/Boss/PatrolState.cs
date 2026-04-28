using UnityEngine;

public class PatrolState : EnemyState
{
    private int currentIndex;
    private float arrivalThreshold = 1.5f;

    public PatrolState(EnemyStateMachine sm) : base(sm) { }

    public override void Enter()
    {
        if (Context.PatrolPoints == null || Context.PatrolPoints.Length == 0)
        {
            StateMachine.ChangeState(new BossIdleState(StateMachine));
            return;
        }

        Context.Agent.isStopped = false;
        currentIndex = 0;
        MoveToCurrentPoint();
    }

    public override void Update()
    {
        if (Context.PatrolPoints == null || Context.PatrolPoints.Length == 0) return;
        if (Context.PlayerTransform == null) return;

        // Если игрок в зоне обнаружения — переход в Idle
        if (PlayerInDetectionRange() && !GameModel.Instance.IsPeacefulMode)
        {
            StateMachine.ChangeState(new BossIdleState(StateMachine));
            return;
        }

        // Проверяем достижение точки
        if (!Context.Agent.pathPending && Context.Agent.remainingDistance <= arrivalThreshold)
        {
            currentIndex = (currentIndex + 1) % Context.PatrolPoints.Length;
            MoveToCurrentPoint();
        }

        Context.Animator.SetFloat("Speed", Context.Agent.velocity.magnitude);
    }

    private void MoveToCurrentPoint()
    {
        Context.Agent.SetDestination(Context.PatrolPoints[currentIndex].position);
    }

    private bool PlayerInDetectionRange()
    {
        float dist = Vector3.Distance(Context.Transform.position, Context.PlayerTransform.position);
        return dist <= Context.Settings.DetectionRange;
    }

    public override void OnDamaged()
    {
        StateMachine.ChangeState(new BossAggressiveState(StateMachine));
    }

    public override void Exit()
    {
        Context.Agent.isStopped = true;
    }
}