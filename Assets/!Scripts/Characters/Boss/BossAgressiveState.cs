using UnityEngine;

public class BossAggressiveState : EnemyState
{
    private float retreatChance = 0.5f;
    private bool canRetreatThisState;

    public BossAggressiveState(EnemyStateMachine sm) : base(sm) { }

    public override void Enter()
    {
        Context.Agent.isStopped = false;
        Context.Animator.SetFloat("Speed", 1f);

        float healthPercent = Context.Health.CurrentHealth / Context.Health.MaxHealth;
        canRetreatThisState = healthPercent < Context.Settings.FleeHealthPercent && Random.value < retreatChance;
    }

    public override void Update()
    {
        if (Context.PlayerTransform == null) return;

        float distance = Vector3.Distance(Context.Transform.position, Context.PlayerTransform.position);

        // Если сработал retreatChance и HP меньше FleeHealthPercent переход в отступление
        if (canRetreatThisState)
        {
            StateMachine.ChangeState(new BossRetreatState(StateMachine));
            return;
        }

        // Если игрок далеко, то переходим в Idle
        if (distance > Context.Settings.DetectionRange)
        {
            StateMachine.ChangeState(new BossIdleState(StateMachine));
            return;
        }

        // Если игрок близко - атакуем
        if (distance <= Context.Settings.AttackRange)
        {
            if (Time.time >= StateMachine.LastAttackTime + Context.Settings.AttackCooldown)
            {
                ChooseAttack();
                return;
            }
        }

        // Иначе следуем за игрком
        MoveTowardsPlayer(distance);
    }

    private void MoveTowardsPlayer(float distance)
    {
        Vector3 dirToPlayer = (Context.PlayerTransform.position - Context.Transform.position).normalized;
        Vector3 targetPos = Context.PlayerTransform.position - dirToPlayer * (Context.Settings.AttackRange * 0.9f);
        Context.Agent.SetDestination(targetPos);
        RotateTowardsPlayer();
        Context.Animator.SetFloat("Speed", Context.Agent.velocity.magnitude);
    }

    private void ChooseAttack()
    {
        float roll = Random.value;
        if (roll < 0.3f)
        {
            // сильная атака
            Context.PerformStrongAttack();
            StateMachine.ChangeState(new BossAttackState(StateMachine, isStrongAttack: true));
        }
        else if (roll < 0.6f)
        {
            // магическая атака
            Context.PerformMagicAttack();
            StateMachine.ChangeState(new BossAttackState(StateMachine, isMagicAttack: true));
        }
        else
        {
            // обычная атака
            Context.PerformAttack();
            StateMachine.ChangeState(new BossAttackState(StateMachine));
        }
    }

    private void RotateTowardsPlayer()
    {
        Vector3 direction = (Context.PlayerTransform.position - Context.Transform.position).normalized;
        direction.y = 0f;
        if (direction.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            Context.Transform.rotation = Quaternion.Slerp(Context.Transform.rotation, targetRotation, Context.Settings.RotationSpeed * Time.deltaTime);
        }
    }

    public override void Exit()
    {
        Context.Agent.isStopped = true;
    }
}