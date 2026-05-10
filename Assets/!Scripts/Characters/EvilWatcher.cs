using System.Collections;
using UnityEngine;
using UnityEngine.AI;


public enum EvilWatcherAttackType
{
    Fireball,
    Needle
}


public class EvilWatcher : MonoBehaviour, IEnemyContext, IEnemySettings
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Animator animator;
    [SerializeField] private NavMeshAgent agent;

    [Header("Attack Settings")]
    [SerializeField] private EvilWatcherAttackType currentAttackType = EvilWatcherAttackType.Fireball;
    [SerializeField] private GameObject needlePrefab;
    [SerializeField] private float needleDelay = 0.1f;
    [SerializeField] private float needleSpeed = 10f;
    [SerializeField] private Transform projectileSpawnPoint;

    [Header("Settings")]
    [SerializeField] private float chaseRange = 15f;
    [SerializeField] private float attackRange = 8f;
    [SerializeField] private float idealRange = 7f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float attackDuration = 2f;
    [SerializeField] private float needleDuration = 10f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float fleeHealthPercent = 0.3f;

    [Header("Events")]
    [SerializeField] private EnemyKilledEventChannelSO enemyKilledEvent;
    [SerializeField] private int scoreValue = 15;


    // IEnemyContext
    public Animator Animator => animator;
    public Animator BeforeSpawnAnimator => null;
    public NavMeshAgent Agent => agent;
    public Transform Transform => transform;
    public Transform PlayerTransform => player;
    public IHealth Health { get; private set; }
    public IEnemySettings Settings => this;
    public bool IsPeacefulMode => GameModel.Instance != null && GameModel.Instance.IsPeacefulMode;
    public Transform[] PatrolPoints { get; }

    // IEnemySettings
    float IEnemySettings.DetectionRange => chaseRange;
    float IEnemySettings.AttackRange => attackRange;
    float IEnemySettings.IdealCombatDistance => idealRange;
    float IEnemySettings.AttackCooldown => attackCooldown;
    float IEnemySettings.AttackDuration => attackDuration;
    float IEnemySettings.RotationSpeed => rotationSpeed;
    float IEnemySettings.FleeHealthPercent => fleeHealthPercent;
    int IEnemySettings.PhysicalDamage
    {
        get => 0;
        set { }
    }

    private IAttackTypeFactory attackTypeFactory;
    public EvilWatcherAttackType CurrentAttackType { get; private set; }


    private EnemyStateMachine stateMachine;

    public void HandleDeath()
    {
        stateMachine?.ChangeState(new DeadState(stateMachine));

        enemyKilledEvent?.RaiseEvent(new KillData {
            IsBoss = false,
            ScoreValue = scoreValue 
        });
    }

    public void HandleRestart()
    {
        stateMachine?.ChangeState(new IdleState(stateMachine));
        agent.isStopped = false;
    }

    private void Awake()
    {
        Health = GetComponent<IHealth>();
        attackTypeFactory = new RandomAttackTypeFactory();
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

        CurrentAttackType = attackTypeFactory.CreateAttackType();
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

    public void PerformAttack() => PerformMagicAttack(); // ” Ёвил ¬отчера только магическа€ атака, обычной нет
    public void PerformStrongAttack() => PerformMagicAttack();
    
    public void PerformMagicAttack() {

        int attackIndex = 1;

        if (currentAttackType == EvilWatcherAttackType.Fireball)
        {
            attackIndex = 1; // индекс дл€ анимации огненного шара
        }

        else
        {
            attackIndex = 2; // индекс дл€ анимации иглы
            StartCoroutine(NeedleAttackSequence());
        }

        animator.SetTrigger("Attack");
        animator.SetInteger("AttackIndex", attackIndex);
    }

    private IEnumerator NeedleAttackSequence()
    {
        animator.SetTrigger("SpinAttack"); // анимаци€ вращени€
        for (int angle = 0; angle < 360; angle += 45)
        {
            if (projectileSpawnPoint != null && needlePrefab != null)
            {
                Quaternion rotation = Quaternion.Euler(0, angle, 0) * projectileSpawnPoint.rotation;
                var needle = Instantiate(needlePrefab, projectileSpawnPoint.position, rotation);
                needle.GetComponent<SwordAttackDetection>()?.Use(); // активаци€ коллайдера дл€ нанесени€ урона 
                Rigidbody rb = needle.GetComponent<Rigidbody>();
                if (rb != null)
                    rb.linearVelocity = needle.transform.forward * needleSpeed;
                Destroy(needle, attackDuration); // уничтожение иглы после окончани€ атаки
            }
            yield return new WaitForSeconds(needleDelay);
        }
    }


    public void OnAttackFinished() { } // анимаци€ сама нанесЄт урон через ивент

    public void PerformSummon() { }
    public void SwitchToMonsterAnimator() { }
    public void SwitchToRuinsAnimator() { }
    public void EnableSwords() { }

    public new Coroutine StartCoroutine(IEnumerator routine)
    {
        return ((MonoBehaviour)this).StartCoroutine(routine);
    }

    public void SetCurrentAttackType(EvilWatcherAttackType attackType)
    {
        currentAttackType = attackType;
    }

    // ¬изуализаци€ радиусов в редакторе
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