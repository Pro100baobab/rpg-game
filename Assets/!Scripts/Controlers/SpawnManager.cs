using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject meleeEnemyPrefab;
    [SerializeField] private GameObject evilWatcherPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private int enemiesToSpawnCount = 5;
    [SerializeField] private Transform[] initialSpawnPoints;

    [Header("Visuals")]
    [SerializeField] private GameObject spawnPointMarkerPrefab;

    private List<Transform> spawnPoints = new List<Transform>();
    private List<GameObject> activeEnemies = new List<GameObject>();

    private void Awake()
    {
        if (initialSpawnPoints != null)
            spawnPoints.AddRange(initialSpawnPoints);
    }

    public void AddSpawnPoint(Vector3 position)
    {
        if (spawnPointMarkerPrefab != null)
        {
            var marker = Instantiate(spawnPointMarkerPrefab, position, Quaternion.identity);
            spawnPoints.Add(marker.transform);
        }
        else
        {
            var go = new GameObject("SpawnPoint");
            go.transform.position = position;
            spawnPoints.Add(go.transform);
        }
    }

    public void SpawnEnemies()
    {
        if (spawnPoints.Count == 0) return;

        int count = Mathf.Min(enemiesToSpawnCount, spawnPoints.Count);
        if (count <= 0) return;

        List<Transform> shuffled = new List<Transform>(spawnPoints);
        Shuffle(shuffled);

        for (int i = 0; i < count; i++)
        {
            Transform point = shuffled[i];

            GameObject prefab = Random.value < 0.5f ? meleeEnemyPrefab : evilWatcherPrefab;
            GameObject instance = Instantiate(prefab, point.position, point.rotation);

            var melee = instance.GetComponent<MeleeEnemy>();
            if (melee != null) melee.SetRandomWeapon();


            var watcher = instance.GetComponent<EvilWatcher>();
            if (watcher != null)
            {
                IAttackTypeFactory factory = new RandomAttackTypeFactory();
                watcher.SetCurrentAttackType(factory.CreateAttackType());
            }

            activeEnemies.Add(instance);
            if (GameModel.Instance != null)
                GameModel.Instance.Enemies.Add(instance);
        }
    }

    private void Shuffle(List<Transform> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int r = Random.Range(i, list.Count);
            (list[i], list[r]) = (list[r], list[i]);
        }
    }

    public void ClearAllEnemies()
    {
        foreach (var enemy in activeEnemies)
        {
            if (enemy != null) 
                Destroy(enemy);
        }
        activeEnemies.Clear();

        if (GameModel.Instance != null)
        {
            GameModel.Instance.Enemies.Clear();
        }
    }

    public void RestoreEnemy(EnemyData data)
    {
        GameObject prefab = null;
        switch (data.enemyType)
        {
            case "Melee":
                prefab = meleeEnemyPrefab;
                break;
            case "EvilWatcher":
                prefab = evilWatcherPrefab;
                break;
        }
        if (prefab == null) return;

        Vector3 pos = new Vector3(data.posX, data.posY, data.posZ);
        GameObject instance = Instantiate(prefab, pos, Quaternion.identity);

        var health = instance.GetComponent<IHealth>();
        health?.SetHealth(data.health);

        var melee = instance.GetComponent<MeleeEnemy>();
        if (melee != null)
        {
            if (data.meleeRightWeaponIndex >= 0 || data.meleeLeftWeaponIndex >= 0)
                melee.SetWeaponsByIndex(data.meleeRightWeaponIndex, data.meleeLeftWeaponIndex);
            else
                melee.SetRandomWeapon();
        }

        var watcher = instance.GetComponent<EvilWatcher>();
        if (watcher != null)
        {
            watcher.SetCurrentAttackType((EvilWatcherAttackType)data.evilWatcherAttackType);
        }

        activeEnemies.Add(instance);
        if (GameModel.Instance != null)
            GameModel.Instance.Enemies.Add(instance);
    }
}