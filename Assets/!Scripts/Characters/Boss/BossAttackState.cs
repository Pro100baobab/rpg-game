using UnityEngine;

public class BossAttackState : EnemyState
{
    private float timer;
    private readonly bool isStrongAttack;
    private readonly bool isMagicAttack;
    private readonly int damageMultiplier = 2;
    private int lastDamageValue;

    public BossAttackState(EnemyStateMachine sm, bool isStrongAttack = false, bool isMagicAttack = false) : base(sm)
    {
        this.isStrongAttack = isStrongAttack;
        this.isMagicAttack = isMagicAttack;

        // Добавить передачу коэффициента из контекста
    }

    public override void Enter()
    {
        Context.Agent.isStopped = true;
        StateMachine.LastAttackTime = Time.time;
        timer = 0f;

        lastDamageValue = Context.Settings.PhysicalDamage;

        if (isStrongAttack)
        {
            Context.Settings.PhysicalDamage *= damageMultiplier;
        }

        Context.EnableSwords();
    }

    public override void Update()
    {
        timer += Time.deltaTime;

        RotateTowardsPlayer();

        if (timer >= Context.Settings.AttackDuration)
        {
            StateMachine.ChangeState(new BossAggressiveState(StateMachine));
        }
    }

    private void RotateTowardsPlayer()
    {
        if (Context.PlayerTransform != null)
        {
            Vector3 direction = (Context.PlayerTransform.position - Context.Transform.position).normalized;
            direction.y = 0f;
            if (direction.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                Context.Transform.rotation = Quaternion.Slerp(Context.Transform.rotation, targetRotation, Context.Settings.RotationSpeed * Time.deltaTime);
            }
        }
    }

    public override void Exit()
    {
        Context.OnAttackFinished();
        Context.Settings.PhysicalDamage = lastDamageValue;
    }
}