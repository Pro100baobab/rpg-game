using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossSummonState : EnemyState
{
    private BossEnemy boss;
    private Coroutine spawnRoutine;
    private float timer;
    private List<GameObject> activePillars = new List<GameObject>();
    private List<Coroutine> pillarRoutines = new List<Coroutine>();

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
        activePillars.Clear();
        pillarRoutines.Clear();

        spawnRoutine = boss.StartCoroutine(SpawnLoop());
    }

    public override void Update()
    {
        timer += Time.deltaTime;

        if (timer >= boss.SummonDuration && spawnRoutine == null)
        {
            StateMachine.ChangeState(new BossAggressiveState(StateMachine));
        }
    }
    public override void Exit()
    {
        // Останавливаем спавн новых колонн
        if (spawnRoutine != null)
        {
            boss.StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }

        // Плавно опускаем все активные колонны
        foreach (var routine in pillarRoutines)
        {
            if (routine != null)
                boss.StopCoroutine(routine);
        }
        pillarRoutines.Clear();

        foreach (var pillar in activePillars)
        {
            if (pillar != null)
                boss.StartCoroutine(ForceDescendAndDestroy(pillar));
        }
        activePillars.Clear();
    }

    private IEnumerator SpawnLoop()
    {
        while (timer < boss.SummonDuration)
        {
            SpawnPillar();
            yield return new WaitForSeconds(boss.SummonSpawnInterval);
        }
        spawnRoutine = null;
    }

    private void SpawnPillar()
    {
        Vector2 randomCircle = Random.insideUnitCircle.normalized * Random.Range(boss.MinSummonRadius, boss.MaxSummonRadius);
        Vector3 targetPos = boss.Transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);
        Vector3 startPos = targetPos - Vector3.up * boss.RiseHeight;
        Quaternion rotation = Quaternion.identity;

        GameObject pillar = Object.Instantiate(boss.PillarPrefab, startPos, rotation);
        var sword = pillar.GetComponent<SwordAttackDetection>();
        if (sword != null)
            sword.Use();
        activePillars.Add(pillar);

        Coroutine routine = boss.StartCoroutine(AnimatePillar(pillar, startPos, targetPos));
        pillarRoutines.Add(routine);
    }

    private IEnumerator AnimatePillar(GameObject pillar, Vector3 startPos, Vector3 targetPos)
    {
        float rise = boss.RiseDuration;
        float stay = boss.PillarStayDuration;
        float fall = boss.FallDuration;

        // Подъём
        float elapsed = 0f;
        while (elapsed < rise)
        {
            if (pillar == null) yield break;
            pillar.transform.position = Vector3.Lerp(startPos, targetPos, elapsed / rise);
            elapsed += Time.deltaTime;
            yield return null;
        }
        if (pillar == null) yield break;
        pillar.transform.position = targetPos;

        // Удержание
        yield return new WaitForSeconds(stay);

        // Опускание
        elapsed = 0f;
        while (elapsed < fall)
        {
            if (pillar == null) yield break;
            pillar.transform.position = Vector3.Lerp(targetPos, startPos, elapsed / fall);
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (pillar != null)
        {
            activePillars.Remove(pillar);
            Object.Destroy(pillar);
        }
    }

    private IEnumerator ForceDescendAndDestroy(GameObject pillar)
    {
        float fall = boss.FallDuration;
        Vector3 currentPos = pillar.transform.position;
        Vector3 targetPos = currentPos - Vector3.up * boss.RiseHeight;
        float elapsed = 0f;
        while (elapsed < fall)
        {
            if (pillar == null) yield break;
            pillar.transform.position = Vector3.Lerp(currentPos, targetPos, elapsed / fall);
            elapsed += Time.deltaTime;
            yield return null;
        }
        if (pillar != null)
            Object.Destroy(pillar);
    }
}