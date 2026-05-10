using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossSummonState : EnemyState
{
    private BossEnemy boss;
    private Coroutine summonRoutine;
    private float timer;

    public BossSummonState(EnemyStateMachine sm) : base(sm) { }

    public override void Enter()
    {
        boss = Context as BossEnemy;
        if (boss == null)
        {
            StateMachine.ChangeState(new BossAggressiveState(StateMachine));
            return;
        }

        Context.Agent.isStopped = true;
        Context.PerformSummon();
        StateMachine.LastSummonTime = Time.time;
        timer = 0f;

        summonRoutine = boss.StartCoroutine(RunSummon());
    }
    private IEnumerator RunSummon()
    {
        yield return boss.SummonFactory.ExecuteSummon(boss, boss.SummonDuration, boss);
        summonRoutine = null;
    }

    public override void Update()
    {
        timer += Time.deltaTime;
        if (timer >= boss.SummonDuration && summonRoutine == null)
        {
            StateMachine.ChangeState(new BossAggressiveState(StateMachine));
        }
    }

    public override void Exit()
    {
        if (summonRoutine != null)
        {
            boss.StopCoroutine(summonRoutine);
            summonRoutine = null;
        }
    }
}