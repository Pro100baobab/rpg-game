using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class MeleeEnemy : MonoBehaviour, IEnemyContext, IEnemySettings, IPhysicalDamageProvider
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Animator animator;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private SwordAttackDetection sword;

    [Header("Settings")]
    [SerializeField] private float detectionRange = 15f;
    [SerializeField] private float attackRange = 3f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float attackDuration = 2.3f;   // длительность анимации атаки
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float fleeHealthPercent = 0.3f; // при 30% HP в мирном режиме убегает
    [Header("Damage")]
    [SerializeField] private int physicalDamage = 10;

    public int PhysicalDamage => physicalDamage;

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
    float IEnemySettings.DetectionRange => detectionRange;
    float IEnemySettings.AttackRange => attackRange;
    float IEnemySettings.IdealCombatDistance => 0f;
    float IEnemySettings.AttackCooldown => attackCooldown;
    float IEnemySettings.AttackDuration => attackDuration;
    float IEnemySettings.RotationSpeed => rotationSpeed;
    float IEnemySettings.FleeHealthPercent => fleeHealthPercent;
    int IEnemySettings.PhysicalDamage { get; set; }
    public void EnableSwords() { }


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

        // ѕодписка на рестарт
        if (EventSystem.Instance != null)
            EventSystem.Instance.OnRestart += HandleRestart;
    }

    private void Update()
    {
        stateMachine?.Update();
    }

    // IEnemyContext методы атаки
    public void PerformAttack()
    {
        sword.Use();
        int attackIndex = Random.Range(1, 9);
        animator.SetInteger("AttackIndex", attackIndex);
        animator.SetTrigger("Attack");
    }

    public void OnAttackFinished()
    {
        sword.NonUse();
    }

    public void PerformStrongAttack() { }
    public void PerformMagicAttack() { }
    public void PerformSummon() { }
    public void SwitchToMonsterAnimator() { }
    public void SwitchToRuinsAnimator() { }

    private void OnDestroy()
    {
        stateMachine?.Dispose();
        if (EventSystem.Instance != null)
            EventSystem.Instance.OnRestart -= HandleRestart;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}