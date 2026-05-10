using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

public interface IElement
{
    Elements CurrentElement { get; }
    void ChangeMeshByElement();
    enum Elements
    {
        GROUND,
        ROCK,
        LAVA,
        ICE,
        SNOW
    }
}

public enum BossAttackType
{
    Melee,
    Ranged
}

public class BossEnemy : MonoBehaviour, IEnemyContext, IEnemySettings, IPhysicalDamageProvider, IElement
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Animator animator;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private SwordAttackDetection swordRight;
    [SerializeField] private SwordAttackDetection swordLeft;
    [SerializeField] private GameObject HPCanvas;
    [SerializeField] private SkinnedMeshRenderer golemMesh;

    [Header("Ranged Attack")]
    [SerializeField] private GameObject stonePrefab;
    [SerializeField] private float stoneSpeed = 15f;
    [SerializeField] private int rangedDamage = 10;
    [SerializeField] private Transform projectileSpawnPoint;

    [Header("Element Summon Prefabs")]
    [SerializeField] private GameObject earthFissurePrefab;
    [SerializeField] private GameObject fireMeteorPrefab;
    [SerializeField] private GameObject iceShardPrefab;
    [SerializeField] private GameObject icePathSpikePrefab;

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
    [SerializeField] private RuntimeAnimatorController monsterAnimatorController;
    [SerializeField] private Avatar monsterAvatar;
    [SerializeField] private GameObject ruins;
    [SerializeField] private Animator beforeSpawnAnimator;
    [SerializeField] private float assembleDuration = 7.15f;

    [Header("Patrol")]
    [SerializeField] private Transform[] patrolPoints;

    [Header("PillarSummon")]
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

    [Header("ElementsSettings")]
    [SerializeField] private IElement.Elements currentElement = IElement.Elements.ROCK;
    [SerializeField] private Material[] listGolemMaterials; // индексы: GROUND, ROCK, LAVA, ICE, SNOW
    [SerializeField] private Material[] listStoneMaterials; // индексы дл€ камн€ дальнего бо€

    [Header("Attack Type")]
    [SerializeField] private BossAttackType currentAttackType = BossAttackType.Melee;

    private IBossProjectileFactory projectileFactory;
    private IElementSummonFactory summonFactory;

    public IElement.Elements CurrentElement => currentElement;
    public IElementSummonFactory SummonFactory => summonFactory ?? (summonFactory = new DefaultElementSummonFactory());
    public BossAttackType CurrentAttackType => currentAttackType;

    public float FarRangedDistance => detectionRange * 0.75f;

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


    // IEnemySettings Ц AttackCooldown зависит от HP
    float IEnemySettings.DetectionRange => detectionRange;
    float IEnemySettings.AttackRange => attackRange;
    float IEnemySettings.IdealCombatDistance => 0f;
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
    int IEnemySettings.PhysicalDamage
    {
        get => physicalDamage;
        set => physicalDamage = value;
    }
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

    // —войства дл€ стихийного Summon
    public GameObject EarthFissurePrefab => earthFissurePrefab;
    public GameObject FireMeteorPrefab => fireMeteorPrefab;
    public GameObject IceShardPrefab => iceShardPrefab;
    public GameObject IcePathSpikePrefab => icePathSpikePrefab;


    public void HandleDeath()
    {
        stateMachine?.ChangeState(new DeadState(stateMachine));
    }

    public void HandleRestart()
    {
        SwitchToRuinsAnimator(); // рестарт начинаетс€ с руин
    }

    private void Awake()
    {
        Health = GetComponent<IHealth>();
        projectileFactory = new BossProjectileFactory(stonePrefab, listStoneMaterials);
        summonFactory = new DefaultElementSummonFactory();
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

        ChangeMeshByElement();
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
    }

    public void SwitchToRuinsAnimator()
    {
        agent.isStopped = true;
        golemMesh.enabled = false;
        ruins.SetActive(true);
        HPCanvas.SetActive(false);

        stateMachine.Initialize(new RuinsState(stateMachine));
    }

    // ћетоды атак и призыва
    public void PerformAttack()
    {
        if (currentAttackType == BossAttackType.Melee)
        {
            EnableSwords();
            int attackIndex = Random.Range(1, 6);
            animator.SetInteger("AttackIndex", attackIndex);
            animator.SetTrigger("Attack");
        }
        else
        {
            PerformMagicAttack(); // дальн€€ атака
        }
    }

    private void PerformRangedAttack()
    {
        if (stonePrefab == null || player == null) return;

        Vector3 offset = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(0f, 1f), Random.Range(-0.5f, 0.5f));
        GameObject stone = Instantiate(stonePrefab, projectileSpawnPoint.position + offset, Quaternion.identity);
        
        // ћен€ем материал камн€ на соответствующий стихии
        MeshRenderer stoneRenderer = stone.GetComponent<MeshRenderer>();
        if (stoneRenderer != null && listStoneMaterials.Length > 0)
        {
            int matIndex = (int)currentElement;
            if (matIndex < listStoneMaterials.Length)
                stoneRenderer.material = listStoneMaterials[matIndex];
        }

        // «апускаем камень в игрока
        Rigidbody rb = stone.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 direction = (new Vector3(player.position.x, player.position.y + Random.Range(0.5f, 1), player.position.z) - projectileSpawnPoint.position).normalized;
            rb.linearVelocity = direction * stoneSpeed;
        }
    }

    public void PerformStrongAttack()
    {
        if (currentAttackType == BossAttackType.Melee)
        {
            EnableSwords();
            int attackIndex = Random.Range(6, 10);
            animator.SetInteger("AttackIndex", attackIndex);
            animator.SetTrigger("Attack");
        }
        else
        {
            PerformMagicAttack(); // дл€ дальнего бо€ сильна€ атака така€ же, как дальн€€
        }
    }

    public void PerformMagicAttack()
    {
        // ћагическа€ атака сейчас используетс€ как дальн€€ атака (индекс 10)
        if (currentAttackType == BossAttackType.Ranged)
        {
            animator.SetInteger("AttackIndex", 10);
            animator.SetTrigger("Attack");

            // PerformRangedAttack(); - вызывать непосредственно здесь не нужно, так как анимаци€ должна синхронизироватьс€ с по€влением камн€ через Animation Event
        }
        else
        {
            PerformAttack();
        }
    }

    public void PerformSummon()
    {
        animator.SetTrigger("Summon");
    }

    public void OnAttackFinished() {
        swordLeft?.NonUse();
        swordRight?.NonUse();
    }

    public void EnableSwords()
    {
        swordLeft?.Use();
        swordRight?.Use();
    }

    public void ChangeMeshByElement()
    {
        int index = (int)currentElement;
        if (listGolemMaterials != null && index < listGolemMaterials.Length)
            golemMesh.material = listGolemMaterials[index];
    }

    public void SetElement(IElement.Elements newElement)
    {
        currentElement = newElement;
        ChangeMeshByElement();
    }

    public void SetAttackType(BossAttackType newType)
    {
        currentAttackType = newType;
    }

    public new Coroutine StartCoroutine(System.Collections.IEnumerator routine)
    {
        return ((MonoBehaviour)this).StartCoroutine(routine);
    }
}