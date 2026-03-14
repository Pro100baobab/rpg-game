using UnityEngine;
using UnityEngine.AI;

public class EvilWatcher : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Animator animator;
    [SerializeField] private NavMeshAgent agent;

    [Header("Settings")]
    [SerializeField] private float chaseRange = 15f;
    [SerializeField] private float attackRange = 8f;
    [SerializeField] private float idealRange = 7f;
    [SerializeField] private float rotationSpeed = 5f;

    [Header("Combat")]
    [SerializeField] private float attackCooldown = 2f;

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
        if (isDead || player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= attackRange)
        {
            // Проверка на кулдаун атаки
            if (Time.time >= lastAttackTime + attackCooldown)
                Attack();

            else
            {
                RotateTowards(player.position);
                HandleMovement(); // Для корректкировки идеального расстояния
            }
        }

        else if (distance <= chaseRange)
            HandleMovement();

        else
        {
            agent.isStopped = true;
            animator.SetFloat("Speed", 0f);
        }
    }

    private void HandleMovement()
    {
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, player.position);

        Vector3 moveDirection;
        if (distance < attackRange)
        {
            moveDirection = -directionToPlayer;
        }

        else if (distance > idealRange)
        {
            moveDirection = directionToPlayer;
        }

        else
        {
            moveDirection = Vector3.zero;
        }

        RotateTowards(player.position);

        if (moveDirection != Vector3.zero)
        {
            agent.isStopped = false;

            Vector3 targetPosition = transform.position + moveDirection;
            agent.SetDestination(targetPosition);

            animator.SetFloat("Speed", agent.velocity.magnitude);
        }
        else
        {
            agent.isStopped = true;
            animator.SetFloat("Speed", 0f);
        }
    }

    // Это всегда магическая атака
    private void Attack()
    {
        int attackIndex = Random.Range(1, 2); // Специально только 1 (в дальнейшем будет расширено)
        animator.SetInteger("AttackIndex", attackIndex);
        animator.SetTrigger("Attack");
        lastAttackTime = Time.time;

        agent.isStopped = true;
    }

    private void RotateTowards(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0f;

        if (direction.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    // Визуализация радиусов в редакторе
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, idealRange);
    }
}