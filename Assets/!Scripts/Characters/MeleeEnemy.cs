using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class MeleeEnemy : MonoBehaviour, IPhysicalDamageProvider
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Animator animator;
    [SerializeField] private NavMeshAgent agent;

    [Header("Settings")]
    [SerializeField] private float detectionRange = 15f;
    [SerializeField] private float attackRange = 3f;
    [SerializeField] private float attackCooldown = 2f;

    [Header("Damage")]
    [SerializeField] private int physicalDamage = 10;
    [SerializeField] private SwordAttackDetection sword;

    public int PhysicalDamage => physicalDamage;

    private float lastAttackTime;
    private bool isDead = false;

    public void Die() => isDead = true;
    public void Revival() => isDead = false;

    private void Start()
    {
        if (player == null) player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (animator == null) animator = GetComponent<Animator>();
        if (agent == null) agent = GetComponent<NavMeshAgent>();

        agent.isStopped = false;
    }

    private void Update()
    {
        if (player == null || isDead) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= attackRange)
        {
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                Attack();
            }
            else
            {
                agent.isStopped = true;
                RotateTowards(player.position);
                animator.SetFloat("Speed", 0f);
            }
        }

        else if (distance <= detectionRange)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
            RotateTowards(player.position);
            animator.SetFloat("Speed", agent.velocity.magnitude);
        }
        else
        {
            agent.isStopped = true;
            animator.SetFloat("Speed", 0f);
        }
    }

    // Это всегда физиечская атака
    private void Attack()
    {
        sword.Use();

        int attackIndex = Random.Range(1, 9);
        animator.SetInteger("AttackIndex", attackIndex);
        animator.SetTrigger("Attack");
        lastAttackTime = Time.time;

        agent.isStopped = true;
        StartCoroutine(ResetSwordUsing());
    }

    private void RotateTowards(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0f;
        if (direction.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }

    private IEnumerator ResetSwordUsing()
    {
        yield return new WaitForSeconds(2.3f);
        sword.NonUse();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}