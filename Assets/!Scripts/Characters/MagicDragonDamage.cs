using UnityEngine;
using System.Collections.Generic;

public class MagicDragonDamage : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private int magicDamage = 50;
    [SerializeField] private float damageRadius = 7f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Gizmos Settings")]
    [SerializeField] private bool showDebugSphere = true;
    [SerializeField] private Color debugSphereColor = Color.red;

    public int MagicDamage => magicDamage;

    // Этот метод будет вызываться из анимации через Animation Event
    public void DealDamageInRadius()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, damageRadius, enemyLayer);

        HashSet<GameObject> damagedEnemies = new HashSet<GameObject>();

        foreach (var hitCollider in hitColliders)
        {
            GameObject enemyObject = hitCollider.gameObject;

            if (damagedEnemies.Contains(enemyObject))
                continue;

            var health = enemyObject.GetComponent<IHealth>();
            if (health != null)
            {
                health.TakeDamage(magicDamage, DamageType.Magic);
                damagedEnemies.Add(enemyObject);

                Debug.Log($"Нанесён магический урон {magicDamage} врагу: {enemyObject.name}");
            }
        }

        Debug.Log($"Атака выполнена. Урон нанесён {damagedEnemies.Count} врагам в радиусе {damageRadius}");
    }

    private void OnDrawGizmosSelected()
    {
        if (showDebugSphere)
        {
            Gizmos.color = debugSphereColor;
            Gizmos.DrawWireSphere(transform.position, damageRadius);
        }
    }
}