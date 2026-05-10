using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MeleeEnemy : MonoBehaviour, IEnemyContext, IEnemySettings, IPhysicalDamageProvider
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Animator animator;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private SwordAttackDetection swordRight;
    [SerializeField] private SwordAttackDetection swordLeft;

    [Header("Weapon Variation (Factory)")]
    [SerializeField] private List<GameObject> rightWeaponPrefabs = new List<GameObject>();
    [SerializeField] private List<GameObject> leftWeaponPrefabs = new List<GameObject>();

    [Header("Settings")]
    [SerializeField] private float detectionRange = 15f;
    [SerializeField] private float attackRange = 3f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float attackDuration = 2.3f;   // длительность анимации атаки
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float fleeHealthPercent = 0.3f; // при 30% HP в мирном режиме убегает
    
    [Header("Damage")]
    [SerializeField] private int physicalDamage = 10;

    [Header("Events")]
    [SerializeField] private EnemyKilledEventChannelSO enemyKilledEvent;
    [SerializeField] private int scoreValue = 10;

    public int PhysicalDamage => physicalDamage;
    public int RightWeaponIndex { get; private set; } = -1;
    public int LeftWeaponIndex { get; private set; } = -1;

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
    int IEnemySettings.PhysicalDamage
    {
        get => physicalDamage;
        set => physicalDamage = value;
    }

    private IWeaponFactory weaponFactory;

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
        weaponFactory = new DefaultWeaponFactory(rightWeaponPrefabs, leftWeaponPrefabs);
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

        SetRandomWeapon();
    }

    private void Update()
    {
        stateMachine?.Update();
    }

    public void SetRandomWeapon()
    {
        // —лучайно выбираем, какую руку вооружить
        bool useRight = Random.value < 0.5f;
        RightWeaponIndex = -1;
        LeftWeaponIndex = -1;

        if (useRight && rightWeaponPrefabs.Count > 0)
        {
            RightWeaponIndex = Random.Range(0, rightWeaponPrefabs.Count);
            var newRight = weaponFactory.CreateRightWeapon(swordRight?.transform.parent);
            if (newRight) ReplaceSword(ref swordRight, newRight);
        }
        else if (!useRight && leftWeaponPrefabs.Count > 0)
        {
            LeftWeaponIndex = Random.Range(0, leftWeaponPrefabs.Count);
            var newLeft = weaponFactory.CreateLeftWeapon(swordLeft?.transform.parent);
            if (newLeft) ReplaceSword(ref swordLeft, newLeft);
        }

        // ќставл€ем вторую руку без оружи€
        if (useRight && swordLeft != null) Destroy(swordLeft.gameObject);
        else if (!useRight && swordRight != null) Destroy(swordRight.gameObject);
    }

    private void ReplaceSword(ref SwordAttackDetection current, GameObject newObj)
    {
        var newSword = newObj.GetComponent<SwordAttackDetection>();
        if (newSword != null && current != null)
        {
            newSword.transform.localPosition = current.transform.localPosition;
            newSword.transform.localRotation = current.transform.localRotation;
            Destroy(current.gameObject);
            current = newSword;
        }
    }

    public void SetWeaponsByIndex(int rightIndex, int leftIndex)
    {
        if (rightIndex >= 0 && rightIndex < rightWeaponPrefabs.Count)
        {
            var newRight = Instantiate(rightWeaponPrefabs[rightIndex], swordRight?.transform.parent);
            ReplaceSword(ref swordRight, newRight);
            RightWeaponIndex = rightIndex;
        }
        else if (swordRight != null)
        {
            Destroy(swordRight.gameObject);
            RightWeaponIndex = -1;
        }

        if (leftIndex >= 0 && leftIndex < leftWeaponPrefabs.Count)
        {
            var newLeft = Instantiate(leftWeaponPrefabs[leftIndex], swordLeft?.transform.parent);
            ReplaceSword(ref swordLeft, newLeft);
            LeftWeaponIndex = leftIndex;
        }
        else if (swordLeft != null)
        {
            Destroy(swordLeft.gameObject);
            LeftWeaponIndex = -1;
        }
    }

    // IEnemyContext методы атаки
    public void PerformAttack()
    {
        swordLeft?.Use();
        swordRight?.Use();
        int attackIndex = Random.Range(1, 9);
        animator.SetInteger("AttackIndex", attackIndex);
        animator.SetTrigger("Attack");
    }

    public void OnAttackFinished()
    {
        swordLeft?.NonUse();
        swordRight?.NonUse();
    }

    public void PerformStrongAttack() => PerformAttack();
    public void PerformMagicAttack() => PerformAttack();
    public void PerformSummon() { }
    public void SwitchToMonsterAnimator() { }
    public void SwitchToRuinsAnimator() { }
    public void EnableSwords() { }

    public new Coroutine StartCoroutine(System.Collections.IEnumerator routine)
    {
        return ((MonoBehaviour)this).StartCoroutine(routine);
    }

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