using UnityEngine;

public class AttackState : EnemyState
{
    private float timer;

    public AttackState(EnemyStateMachine sm) : base(sm) { }

    public override void Enter()
    {
        Context.Agent.isStopped = true;
        Context.PerformAttack();
        StateMachine.LastAttackTime = Time.time;
        timer = 0f;
    }

    public override void Update()
    {
        timer += Time.deltaTime;

        // ѕоворачиваемс€ на игрока во врем€ атаки
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

        if (timer >= Context.Settings.AttackDuration)
        {
            Context.OnAttackFinished();
            float distance = Context.PlayerTransform != null
                ? Vector3.Distance(Context.Transform.position, Context.PlayerTransform.position)
                : float.MaxValue;

            if (Context.IsPeacefulMode)
            {
                float healthPercent = Context.Health.CurrentHealth / Context.Health.MaxHealth;
                if (healthPercent <= Context.Settings.FleeHealthPercent)
                    StateMachine.ChangeState(new FleeState(StateMachine));
                else
                    StateMachine.ChangeState(new IdleState(StateMachine));
            }
            else if (distance > Context.Settings.DetectionRange)
            {
                StateMachine.ChangeState(new IdleState(StateMachine));
            }
            else
            {
                StateMachine.ChangeState(new AggressiveState(StateMachine));
            }
        }
    }

    public override void Exit()
    {
        Context.OnAttackFinished();
    }
}