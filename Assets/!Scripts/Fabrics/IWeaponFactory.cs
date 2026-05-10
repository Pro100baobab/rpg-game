using System.Collections.Generic;
using UnityEngine;


// Абстрактный метод и фабрика по умолчанию (для MeelEnemy)
public interface IWeaponFactory
{
    GameObject CreateRightWeapon(Transform parent);
    GameObject CreateLeftWeapon(Transform parent);
}

public class DefaultWeaponFactory : IWeaponFactory
{
    private readonly List<GameObject> rightPrefabs;
    private readonly List<GameObject> leftPrefabs;

    public DefaultWeaponFactory(List<GameObject> rightPrefabs, List<GameObject> leftPrefabs)
    {
        this.rightPrefabs = rightPrefabs;
        this.leftPrefabs = leftPrefabs;
    }

    public GameObject CreateRightWeapon(Transform parent)
    {
        if (rightPrefabs.Count == 0) return null;
        var prefab = rightPrefabs[Random.Range(0, rightPrefabs.Count)];
        return Object.Instantiate(prefab, parent.position, parent.rotation, parent);
    }

    public GameObject CreateLeftWeapon(Transform parent)
    {
        if (leftPrefabs.Count == 0) return null;
        var prefab = leftPrefabs[Random.Range(0, leftPrefabs.Count)];
        return Object.Instantiate(prefab, parent.position, parent.rotation, parent);
    }
}