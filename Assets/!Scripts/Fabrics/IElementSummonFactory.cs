using System.Collections;
using UnityEngine;


// Абстрактный метод и фабрика по умолчанию для призыва стихий (для BossEnemy)
public interface IElementSummonFactory
{
    IEnumerator ExecuteSummon(BossEnemy boss, float summonDuration, MonoBehaviour coroutineRunner);
}

public class DefaultElementSummonFactory : IElementSummonFactory
{
    public IEnumerator ExecuteSummon(BossEnemy boss, float summonDuration, MonoBehaviour coroutineRunner)
    {
        switch (boss.CurrentElement)
        {
            case IElement.Elements.GROUND:
                yield return coroutineRunner.StartCoroutine(EarthFissure(boss));
                break;
            case IElement.Elements.ROCK:
                yield return coroutineRunner.StartCoroutine(RockPillars(boss, summonDuration));
                break;
            case IElement.Elements.LAVA:
                yield return coroutineRunner.StartCoroutine(FireMeteor(boss));
                break;
            case IElement.Elements.ICE:
                yield return coroutineRunner.StartCoroutine(IcePathSpike(boss));
                break;
            case IElement.Elements.SNOW:
                yield return coroutineRunner.StartCoroutine(IceShard(boss));
                break;
        }
    }

    private IEnumerator EarthFissure(BossEnemy boss)
    {
        for (int angle = 0; angle < 360; angle += 45)
        {
            Vector3 direction = Quaternion.Euler(0, angle, 0) * boss.Transform.forward;
            if (boss.EarthFissurePrefab != null)
            {
                Object.Instantiate(boss.EarthFissurePrefab, boss.Transform.position, Quaternion.LookRotation(direction));
            }
            yield return new WaitForSeconds(boss.SummonSpawnInterval);
        }
    }

    private IEnumerator RockPillars(BossEnemy boss, float summonDuration)
    {
        var activePillars = new System.Collections.Generic.List<GameObject>();
        float timer = 0f;
        while (timer < summonDuration)
        {
            SpawnPillar(boss, activePillars);
            yield return new WaitForSeconds(boss.SummonSpawnInterval);
            timer += boss.SummonSpawnInterval;
        }
        // Ожидаем, пока все колонны завершат цикл (они сами удалятся)
        yield return new WaitForSeconds(boss.RiseDuration + boss.PillarStayDuration + boss.FallDuration);
        // Дополнительная очистка на случай преждевременного выхода
        foreach (var pillar in activePillars)
        {
            if (pillar != null) Object.Destroy(pillar);
        }
    }

    private void SpawnPillar(BossEnemy boss, System.Collections.Generic.List<GameObject> activePillars)
    {
        Vector2 randomCircle = Random.insideUnitCircle.normalized * Random.Range(boss.MinSummonRadius, boss.MaxSummonRadius);
        Vector3 targetPos = boss.Transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);
        Vector3 startPos = targetPos - Vector3.up * boss.RiseHeight;
        GameObject pillar = Object.Instantiate(boss.PillarPrefab, startPos, Quaternion.identity);
        activePillars.Add(pillar);
        boss.StartCoroutine(AnimatePillar(boss, pillar, startPos, targetPos, activePillars));
    }

    private IEnumerator AnimatePillar(BossEnemy boss, GameObject pillar, Vector3 startPos, Vector3 targetPos, System.Collections.Generic.List<GameObject> activePillars)
    {
        float rise = boss.RiseDuration;
        float stay = boss.PillarStayDuration;
        float fall = boss.FallDuration;

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

        yield return new WaitForSeconds(stay);

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

    private IEnumerator FireMeteor(BossEnemy boss)
    {
        Vector3 basePos = boss.Transform.position;
        Vector3 toPlayer = (boss.PlayerTransform.position - basePos).normalized;
        Vector3 right = Vector3.Cross(Vector3.up, toPlayer).normalized;

        float spacing = 2f;
        for (int i = 0; i < 3; i++)
        {
            Vector3 offset = (i - 1) * spacing * right + ((i == 1) ? -1f : 0f) * toPlayer;
            if (boss.FireMeteorPrefab != null)
            {
                Object.Instantiate(boss.FireMeteorPrefab, boss.PlayerTransform.position + offset, Quaternion.LookRotation(toPlayer));
            }
        }
        yield return new WaitForSeconds(boss.SummonDuration);
    }

    private IEnumerator IcePathSpike(BossEnemy boss)
    {
        Vector3 toPlayer = (boss.PlayerTransform.position - boss.Transform.position).normalized;
        float[] angles = { -30f, 0f, 30f };
        foreach (float angle in angles)
        {
            Vector3 dir = Quaternion.Euler(0, angle, 0) * toPlayer;
            if (boss.IcePathSpikePrefab != null)
            {
                Object.Instantiate(boss.IcePathSpikePrefab, boss.Transform.position, Quaternion.LookRotation(dir));
            }
        }
        yield return new WaitForSeconds(boss.SummonDuration);
    }

    private IEnumerator IceShard(BossEnemy boss)
    {
        Vector3 basePos = boss.Transform.position;
        Vector3 toPlayer = (boss.PlayerTransform.position - basePos).normalized;
        Vector3 right = Vector3.Cross(Vector3.up, toPlayer).normalized;

        float spacing = 2f;
        for (int i = 0; i < 3; i++)
        {
            Vector3 offset = (i - 1) * spacing * right + ((i == 1) ? -1f : 0f) * toPlayer;
            if (boss.IceShardPrefab != null)
            {
                Object.Instantiate(boss.IceShardPrefab, boss.PlayerTransform.position + offset, Quaternion.LookRotation(toPlayer));
            }
        }
        yield return new WaitForSeconds(boss.SummonDuration);
    }
}