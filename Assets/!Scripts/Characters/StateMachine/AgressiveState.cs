using UnityEngine;

public class AggressiveState : EnemyState
{
    public AggressiveState(EnemyStateMachine sm) : base(sm) { }

    public override void Enter()
    {
        Context.Agent.isStopped = false;
        Context.Animator.SetFloat("Speed", 1f);
    }

    public override void Update()
    {
        if (Context.PlayerTransform == null) return;

        float distance = Vector3.Distance(Context.Transform.position, Context.PlayerTransform.position);

        if (Context.IsPeacefulMode)
        {
            float healthPercent = Context.Health.CurrentHealth / Context.Health.MaxHealth;
            if (healthPercent <= Context.Settings.FleeHealthPercent)
            {
                StateMachine.ChangeState(new FleeState(StateMachine));
            }
            else
            {
                StateMachine.ChangeState(new IdleState(StateMachine));
            }
            return;
        }

        if (distance > Context.Settings.DetectionRange)
        {
            StateMachine.ChangeState(new IdleState(StateMachine));
            return;
        }

        // Проверка на атаку
        if (distance <= Context.Settings.AttackRange)
        {
            if (Time.time >= StateMachine.LastAttackTime + Context.Settings.AttackCooldown)
            {
                StateMachine.ChangeState(new AttackState(StateMachine));
                return;
            }
        }

        // Движение к идеальной позиции
        Vector3 dirToPlayer = (Context.PlayerTransform.position - Context.Transform.position).normalized;
        float ideal = Context.Settings.IdealCombatDistance > 0 ? Context.Settings.IdealCombatDistance : Context.Settings.AttackRange * 0.5f;
        Vector3 targetPos;

        if (distance > ideal)
            targetPos = Context.PlayerTransform.position - dirToPlayer * ideal;
        else if (distance < ideal - 0.5f)
            targetPos = Context.PlayerTransform.position + dirToPlayer * ideal;
        else
        {
            Context.Agent.isStopped = true;
            Context.Animator.SetFloat("Speed", 0f);
            RotateTowardsPlayer();
            return;
        }

        Context.Agent.isStopped = false;
        Context.Agent.SetDestination(targetPos);
        RotateTowardsPlayer();
        Context.Animator.SetFloat("Speed", Context.Agent.velocity.magnitude);
    }

    public override void Exit()
    {
        Context.Agent.isStopped = true;
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
}