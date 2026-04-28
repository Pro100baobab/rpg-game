using UnityEngine;

public class SwordAttackDetection : MonoBehaviour
{
    [Tooltip("Значение урона по умолчанию, если не найден IPhysicalDamageProvider")]
    [SerializeField] private int defaultSwordForce = 10;
    [SerializeField] private int swordForce;

    private Collider swordCollider;
    private IPhysicalDamageProvider damageProvider;
    private float delay = 1f;
    private float lastUsedTime = 0f;
    private bool isDelay = false;
    private bool isUsing = false;

    public bool IsUsing => isUsing;
    public void Use()
    {
        isUsing = true;

        if (swordCollider != null)
            swordCollider.enabled = true;
    }
    public void NonUse()
    {
        isUsing = false;
        
        if (swordCollider!= null)
            swordCollider.enabled = false;
    }


    private void Start()
    {
        swordCollider = GetComponent<MeshCollider>();

        damageProvider = transform.root.GetComponent<IPhysicalDamageProvider>();

        if (damageProvider != null)
            swordForce = damageProvider.PhysicalDamage;

        else
        {
            swordForce = defaultSwordForce;
            Debug.Log("используются дефолтные значения");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (lastUsedTime + delay >= Time.time) return;
        else isDelay = false;

        if (isDelay || !isUsing) return;


        var health = other.GetComponent<IHealth>();

        if (health != null)
        {
            swordForce = damageProvider != null ? 
                damageProvider.PhysicalDamage : defaultSwordForce;

            health.TakeDamage(swordForce, DamageType.Physical);
        }

        isDelay = true;
        lastUsedTime = Time.time;
    }

}
