using UnityEngine;

public class BossSummonState : EnemyState
{
    private float summonDuration = 3f;
    private float timer;

    public BossSummonState(EnemyStateMachine sm) : base(sm) { }

    public override void Enter()
    {
        Context.Agent.isStopped = true;
        Context.PerformSummon();
        timer = 0f;
    }

    public override void Update()
    {
        timer += Time.deltaTime;

        if (timer >= summonDuration)
        {
            StateMachine.ChangeState(new BossAggressiveState(StateMachine));
        }
    }
}