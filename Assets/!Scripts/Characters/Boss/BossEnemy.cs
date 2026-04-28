using UnityEngine;
using UnityEngine.AI;

public class BossEnemy : MonoBehaviour, IEnemyContext, IEnemySettings, IPhysicalDamageProvider
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Animator animator;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private SwordAttackDetection swordRight;
    [SerializeField] private SwordAttackDetection swordLeft;
    [SerializeField] private GameObject HPCanvas;
    [SerializeField] private SkinnedMeshRenderer golemMesh;


    [Header("Boss Settings")]
    [SerializeField] private float detectionRange = 20f;
    [SerializeField] private float attackRange = 5f;
    [SerializeField] private int physicalDamage = 5;
    [SerializeField] private float baseAttackCooldown = 6f;
    [SerializeField] private float attackDuration = 2.5f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float fleeHealthPercent = 0.2f; // дл€ Retreat
    [SerializeField] private float phase2CooldownMultiplier = 0.5f;


    [Header("Animator Switch")]
    // [SerializeField] private RuntimeAnimatorController ruinsAnimatorController;
    // [SerializeField] private Avatar ruinsAvatar;
    [SerializeField] private RuntimeAnimatorController monsterAnimatorController;
    [SerializeField] private Avatar monsterAvatar;
    [SerializeField] private GameObject ruins;
    [SerializeField] private Animator beforeSpawnAnimator;
    [SerializeField] private float assembleDuration = 7.15f;

    [Header("Patrol")]
    [SerializeField] private Transform[] patrolPoints;

    [Header("Summon")]
    [SerializeField] private GameObject pillarPrefab;
    [SerializeField] private float summonCooldown = 20f;
    [SerializeField] private float summonChance = 0.4f;
    [SerializeField] private float summonDuration = 5f;
    [SerializeField] private float minSummonRadius = 3f;
    [SerializeField] private float maxSummonRadius = 8f;
    [SerializeField] private float riseHeight = 4f;
    [SerializeField] private float riseDuration = 1f;
    [SerializeField] private float pillarStayDuration = 1f;
    [SerializeField] private float fallDuration = 1f;
    [SerializeField] private float summonSpawnInterval = 0.5f;


    // IEnemyContext
    public Animator Animator => animator;
    public Animator BeforeSpawnAnimator => beforeSpawnAnimator;
    public NavMeshAgent Agent => agent;
    public Transform Transform => transform;
    public Transform PlayerTransform => player;
    public IHealth Health { get; private set; }
    public IEnemySettings Settings => this;
    public bool IsPeacefulMode => GameModel.Instance != null && GameModel.Instance.IsPeacefulMode;
    public Transform[] PatrolPoints => patrolPoints;
    // public SwordAttackDetection LeftSwordHand => swordLeft;
    // public SwordAttackDetection RightSwordHand => swordRight;

    // IEnemySettings Ц AttackCooldown зависит от HP
    float IEnemySettings.DetectionRange => detectionRange;
    float IEnemySettings.AttackRange => attackRange;
    float IEnemySettings.IdealCombatDistance => 0f; // босс идЄт пр€мо на игрока
    float IEnemySettings.AttackCooldown
    {
        get
        {
            float mult = (Health != null && Health.CurrentHealth / Health.MaxHealth < 0.5f) ? phase2CooldownMultiplier : 1f;
            return baseAttackCooldown * mult;
        }
    }
    float IEnemySettings.AttackDuration => attackDuration;
    float IEnemySettings.RotationSpeed => rotationSpeed;
    float IEnemySettings.FleeHealthPercent => fleeHealthPercent;
    int IEnemySettings.PhysicalDamage { get  => physicalDamage; set {
            physicalDamage = value;
        } }
    int IPhysicalDamageProvider.PhysicalDamage => physicalDamage;

    private EnemyStateMachine stateMachine;

    // —войства дл€ SummonState
    public GameObject PillarPrefab => pillarPrefab;
    public float SummonCooldown => summonCooldown;
    public float SummonChance => summonChance;
    public float SummonDuration => summonDuration;
    public float MinSummonRadius => minSummonRadius;
    public float MaxSummonRadius => maxSummonRadius;
    public float RiseHeight => riseHeight;
    public float RiseDuration => riseDuration;
    public float PillarStayDuration => pillarStayDuration;
    public float FallDuration => fallDuration;
    public float SummonSpawnInterval => summonSpawnInterval;


    public void HandleDeath()
    {
        stateMachine?.ChangeState(new DeadState(stateMachine));
    }

    public void HandleRestart()
    {
        SwitchToRuinsAnimator(); // рестарт начинаетс€ с руин
        // stateMachine?.ChangeState(new BossIdleState(stateMachine));
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
        SwitchToRuinsAnimator();

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

    public void SwitchToMonsterAnimator()
    {
        agent.isStopped = false;
        HPCanvas.SetActive(true);
        ruins.SetActive(false);
        golemMesh.enabled = true;
        stateMachine?.ChangeState(new BossIdleState(stateMachine));
        // animator.runtimeAnimatorController = monsterAnimatorController;
        // animator.avatar = monsterAvatar;
    }

    public void SwitchToRuinsAnimator()
    {
        agent.isStopped = true;
        golemMesh.enabled = false;
        ruins.SetActive(true);
        HPCanvas.SetActive(false);
        stateMachine.Initialize(new RuinsState(stateMachine));
        // animator.runtimeAnimatorController = ruinsAnimatorController;
        // animator.avatar = ruinsAvatar;
    }

    // ћетоды атак и призыва
    public void PerformAttack()
    {
        EnableSwords();

        int attackIndex = Random.Range(1, 6); // обычна€ атака
        animator.SetInteger("AttackIndex", attackIndex);
        animator.SetTrigger("Attack");
    }

    public void PerformStrongAttack()
    {
        EnableSwords();

        int attackIndex = Random.Range(6, 10); // сильна€ атака
        animator.SetInteger("AttackIndex", attackIndex);
        animator.SetTrigger("Attack");
    }

    public void PerformMagicAttack()
    {
        EnableSwords();

        int attackIndex = 10; // магическа€ атака
        animator.SetInteger("AttackIndex", attackIndex);
        animator.SetTrigger("Attack");
    }

    public void PerformSummon() // пока что не используетс€
    {
        animator.SetTrigger("Summon");
        // призыв миньонов
    }

    public void OnAttackFinished() {
        swordLeft.NonUse();
        swordRight.NonUse();
    }

    public void EnableSwords()
    {
        swordLeft.Use();
        swordRight.Use();
    }
}