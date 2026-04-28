using UnityEngine;
using UnityEngine.AI;

public class EvilWatcher : MonoBehaviour, IEnemyContext, IEnemySettings
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Animator animator;
    [SerializeField] private NavMeshAgent agent;

    [Header("Settings")]
    [SerializeField] private float chaseRange = 15f;
    [SerializeField] private float attackRange = 8f;
    [SerializeField] private float idealRange = 7f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float attackDuration = 2f;      // длительность анимации атаки
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float fleeHealthPercent = 0.3f;

    // IEnemyContext
    public Transform[] PatrolPoints { get; }
    public Animator Animator => animator;
    public Animator BeforeSpawnAnimator => null;
    public NavMeshAgent Agent => agent;
    public Transform Transform => transform;
    public Transform PlayerTransform => player;
    public IHealth Health { get; private set; }
    public IEnemySettings Settings => this;
    public bool IsPeacefulMode => GameModel.Instance != null && GameModel.Instance.IsPeacefulMode;

    // IEnemySettings
    float IEnemySettings.DetectionRange => chaseRange;
    float IEnemySettings.AttackRange => attackRange;
    float IEnemySettings.IdealCombatDistance => idealRange;
    float IEnemySettings.AttackCooldown => attackCooldown;
    float IEnemySettings.AttackDuration => attackDuration;
    float IEnemySettings.RotationSpeed => rotationSpeed;
    float IEnemySettings.FleeHealthPercent => fleeHealthPercent;
    int IEnemySettings.PhysicalDamage { get; set; }


    private EnemyStateMachine stateMachine;

    public void HandleDeath()
    {
        stateMachine?.ChangeState(new DeadState(stateMachine));
    }

    public void HandleRestart()
    {
        stateMachine?.ChangeState(new IdleState(stateMachine));
        agent.isStopped = false;
    }

    private void Awake()
    {
        Health = GetComponent<IHealth>();
    }

    private void Start()
    {
        if (player == null) player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (animator == null) animator = GetComponent<Animator>();
        if (agent == null) agent = GetComponent<NavMeshAgent>();

        stateMachine = new EnemyStateMachine(this);
        stateMachine.Initialize(new IdleState(stateMachine));

        if (EventSystem.Instance != null)
            EventSystem.Instance.OnRestart += HandleRestart;
    }


    private void Update()
    {
        stateMachine?.Update();
    }

    private void OnDestroy()
    {
        stateMachine?.Dispose();
        if (EventSystem.Instance != null)
            EventSystem.Instance.OnRestart -= HandleRestart;
    }

    public void PerformAttack()
    {
        int attackIndex = Random.Range(1, 2);
        animator.SetInteger("AttackIndex", attackIndex);
        animator.SetTrigger("Attack");
    }

    public void PerformStrongAttack() { }
    public void PerformMagicAttack() { }
    public void OnAttackFinished() { } // анимация сама нанесёт урон через ивент
    public void PerformSummon() { }
    public void SwitchToMonsterAnimator() { }
    public void SwitchToRuinsAnimator() { }
    public void EnableSwords() { }


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