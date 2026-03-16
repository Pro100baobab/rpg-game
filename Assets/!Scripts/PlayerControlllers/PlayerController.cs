using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : MonoBehaviour, IPhysicalDamageProvider
{
    [Header("References")]
    private CharacterController controller;
    private Animator animator;
    private PlayerControlls controls;

    [Header("Movement")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 8f;
    public float rotationSpeed = 10f;
    public float jumpHeight = 1.5f;
    public float gravity = -9.81f;

    [Header("Camera")]
    public Transform cameraHolder;
    public float mouseSensitivity = 100f;
    private float xRotation = 0f;

    [Header("Roll")]
    public float rollForce = 5f;
    public float rollDuration = 0.5f;

    [Header("Animation Smoothing")]
    public float velSmoothTime = 0.1f; // Время сглаживания для VelX / VelZ

    [Header("MagicAttackEffects")]
    [SerializeField] private GameObject particlesParent;
    [SerializeField] private GameObject DragonEffect;
    [SerializeField] private int cooldownTime = 30;

    [Header("Damage Settings")]
    [SerializeField] private int physicalDamage = 20;
    [SerializeField] private int magicalDamage = 80;
    [SerializeField] private SwordAttackDetection sword;


    public int PhysicalDamage => physicalDamage;
    public int MagicalDamage => magicalDamage;
    public int CooldownTime => cooldownTime;

    // Input
    private Vector2 lookInput;
    private bool isSprinting;
    private bool isRolling;
    private float currentSpeed;
    private float verticalVelocity;
    private bool isGrounded;
    private bool isCoolDowm = false;

    // Новые оси ввода
    private float forwardInput;    // W/S
    private float turnInput;        // A/D
    private float strafeInput;      // Q/E

    // Сглаженные значения для аниматора
    private float smoothVelX;
    private float smoothVelZ;
    private float velXSmoothVelocity;
    private float velZSmoothVelocity;

    private float coolDownBeginTime;
    private Coroutine cooldownCoroutine;

    #region Unity Methods
    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        controls = new PlayerControlls();

        SetUpInput();
    }

    private void OnEnable() => controls.Gameplay.Enable();

    private void FixedUpdate()
    {
        HandleMovement();
        HandleAnimations();
    }

    private void LateUpdate()
    {
        // Камера (от мыши)
        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;

        mouseX = Mathf.Clamp(mouseX, -1f, 1f);
        mouseY = Mathf.Clamp(mouseY, -1f, 1f);

        // Накопление вертикального поворота
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, 0f, 0f);

        Quaternion horizontalRotation = Quaternion.Euler(0, mouseX, 0);
        cameraHolder.rotation = horizontalRotation * cameraHolder.rotation;

        cameraHolder.GetChild(0).localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    private void OnDisable()
    {
        controls.Gameplay.Disable();

        if (cooldownCoroutine != null)
        {
            StopCoroutine(cooldownCoroutine);
            cooldownCoroutine = null;
            isCoolDowm = false;
        }
            
    }

    #endregion

    #region Used By Animator
    /// <summary>
    /// Методы вызываются в событиях анимаций
    /// </summary>
    public void ChangeVisual(int isOnScreen)
    {
        ParticleSystem[] particles = particlesParent.GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem p in particles)
        {
            if (isOnScreen == 1) p.Play();
            else p.Stop();
        }
    }

    public void ResetAbilityCooldown()
    {
        EventSystem.Instance.AbilityCooldown(cooldownTime);

        if (cooldownCoroutine != null)
            StopCoroutine(cooldownCoroutine);

        cooldownCoroutine = StartCoroutine(ResetCoolDown());
    }

    public void ShowDragon()
    {
        // сброс
        DragonEffect.transform.SetParent(transform);
        DragonEffect.transform.localPosition = Vector3.zero;
        DragonEffect.SetActive(false);

        // активация
        DragonEffect.SetActive(true);
        DragonEffect.transform.SetParent(null);
    }
    #endregion

    private void SetUpInput()
    {
        // Привязка новых действий
        controls.Gameplay.MoveVertical.performed += ctx => forwardInput = ctx.ReadValue<float>();
        controls.Gameplay.MoveVertical.canceled += ctx => forwardInput = 0f;

        controls.Gameplay.Turn.performed += ctx => turnInput = ctx.ReadValue<float>();
        controls.Gameplay.Turn.canceled += ctx => turnInput = 0f;

        controls.Gameplay.Strafe.performed += ctx => strafeInput = ctx.ReadValue<float>();
        controls.Gameplay.Strafe.canceled += ctx => strafeInput = 0f;

        // Остальные действия
        controls.Gameplay.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        controls.Gameplay.Look.canceled += ctx => lookInput = Vector2.zero;

        controls.Gameplay.Sprint.performed += ctx => isSprinting = true;
        controls.Gameplay.Sprint.canceled += ctx => isSprinting = false;

        controls.Gameplay.Jump.performed += OnJump;
        controls.Gameplay.Roll.performed += OnRoll;

        // Заглушки для атак
        controls.Gameplay.PhysicAttack.performed += _ => OnAttack(false); // физическая
        controls.Gameplay.MagicAttack.performed += _ => OnAttack(true); // магическая
    }

    private void HandleMovement()
    {
        isGrounded = controller.isGrounded;

        if (isRolling) return;

        // Поворот персонажа от A/D
        transform.Rotate(Vector3.up * turnInput * rotationSpeed * Time.deltaTime);

        bool isAttacking = animator.GetBool("IsAttacking");
        if (isAttacking) return;

        sword.NonUse();
        UpdateSprintingStatus();

        // Локальное направление: вперёд/назад (W/S) + влево/вправо (Q/E)
        Vector3 moveDirection = (transform.forward * forwardInput + transform.right * strafeInput).normalized;
        if (moveDirection.magnitude > 0.1f)
        {
            currentSpeed = isSprinting ? sprintSpeed : walkSpeed;
            Vector3 motion = moveDirection * (currentSpeed * Time.deltaTime);

            // Вертикаль (гравитация + прыжок)
            if (controller.isGrounded && verticalVelocity < 0)
                verticalVelocity = -2f;
            verticalVelocity += gravity * Time.deltaTime;
            motion.y = verticalVelocity * Time.deltaTime;

            controller.Move(motion);
        }
        else
        {
            // Если нет движения по горизонтали, обрабатываем только вертикаль
            if (controller.isGrounded && verticalVelocity < 0)
                verticalVelocity = -2f;
            verticalVelocity += gravity * Time.deltaTime;
            controller.Move(new Vector3(0, verticalVelocity * Time.deltaTime, 0));
        }
    }

    private void HandleAnimations()
    {
        // Получаем локальную скорость персонажа
        Vector3 localVel = transform.InverseTransformDirection(controller.velocity);

        // Нормализуем относительно скорости ходьбы
        float targetVelX = localVel.x / walkSpeed;
        float targetVelZ = localVel.z / walkSpeed;

        // Плавное изменение параметров
        smoothVelX = Mathf.SmoothDamp(smoothVelX, targetVelX, ref velXSmoothVelocity, velSmoothTime);
        smoothVelZ = Mathf.SmoothDamp(smoothVelZ, targetVelZ, ref velZSmoothVelocity, velSmoothTime);

        // Передаём значения в аниматор
        animator.SetFloat("VelX", smoothVelX);
        animator.SetFloat("VelZ", smoothVelZ);

        animator.SetBool("IsGrounded", isGrounded);
        animator.SetBool("IsRolling", isRolling);
    }

    private void OnJump(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && controller.isGrounded && !isRolling)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -5f * gravity);
            animator.SetTrigger("Jump");
        }
    }

    private void OnRoll(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && !isRolling && controller.isGrounded)
        {
            StartCoroutine(RollCoroutine());
        }
    }

    private void OnAttack(bool isMagic)
    {
        sword.Use();

        if (animator.GetBool("IsAttacking")) return;
        if (isRolling || !isGrounded) return;

        if (isMagic && isCoolDowm) return;
        if (isMagic) isCoolDowm = true;

        int attackIndex = isMagic ? 100 : Random.Range(1, 4);

        animator.SetInteger("AttackIndex", attackIndex);
        animator.SetBool("IsAttacking", true);
        animator.SetTrigger("Attack");

        Debug.Log(isMagic ? "Магическая атака" : "Физическая атака");
    }

    private IEnumerator ResetCoolDown()
    {
        coolDownBeginTime = Time.time;

        while (coolDownBeginTime + cooldownTime > Time.time)
        {
            yield return new WaitForSeconds(1);
        }

        isCoolDowm = false;
    }

    private IEnumerator RollCoroutine()
    {
        isRolling = true;
        animator.SetBool("IsRolling", true);

        float timer = 0;
        Vector3 rollDirection = transform.forward;
        while (timer < rollDuration)
        {
            controller.Move(rollDirection * rollForce * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
        }
        isRolling = false;
        animator.SetBool("IsRolling", false);
    }

    private void UpdateSprintingStatus()
    {
        if (isSprinting)
            animator.SetBool("isSprinting", true);
        else
            animator.SetBool("isSprinting", false);
    }

    // Методы и корруина для редактирования кулдауна по последнему сохранению
    public float GetRemainingCooldown()
    {
        if (!isCoolDowm || cooldownCoroutine == null) return 0f;
        float elapsed = Time.time - coolDownBeginTime;
        return Mathf.Max(0f, cooldownTime - elapsed);
    }

    public void SetRemainingCooldown(float remaining)
    {
        if (cooldownCoroutine != null)
        {
            StopCoroutine(cooldownCoroutine);
            cooldownCoroutine = null;
        }

        if (remaining > 0)
        {
            isCoolDowm = true;
            cooldownCoroutine = StartCoroutine(CooldownWithTime(remaining));
        }
        else
        {
            isCoolDowm = false;
        }
    }

    private IEnumerator CooldownWithTime(float duration)
    {
        float startTime = Time.time;
        while (Time.time < startTime + duration)
        {
            yield return null;
        }
        isCoolDowm = false;
        cooldownCoroutine = null;
    }
}