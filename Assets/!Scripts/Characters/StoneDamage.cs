using UnityEngine;


public class StoneDamage : MonoBehaviour, IPhysicalDamageProvider
{
    public int damage = 10;
    public int PhysicalDamage => damage;

    private void OnTriggerEnter(Collider collision)
    {
        Debug.Log($"Stone collided with {collision.transform.root.gameObject.name}, dealing {damage} physical damage.");

        var health = collision.transform.root.gameObject.GetComponent<IHealth>();
        if (health != null && collision.transform.root.gameObject.tag == "Player")
        {
            health.TakeDamage(damage, DamageType.Physical);
        }
    }
}
