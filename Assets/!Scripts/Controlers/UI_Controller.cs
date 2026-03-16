using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Controller : MonoBehaviour
{
    [Header("Player Health")]
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private Image playerHealthImage;
    [SerializeField] private string healthFillProperty = "_FillAmount";
    private Material playerHealthMaterial;
    private IHealth playerHealth;

    [Header("Panels")]
    [SerializeField] private GameObject RestartPanel;

    [Header("Cooldown Settings")]
    [SerializeField] private Image cooldownImage;
    [SerializeField] private string progressProperty = "_Progress";

    private Material cooldownMaterial;
    private Coroutine cooldownCoroutine;

    void Start()
    {
        if (EventSystem.Instance != null)
        {
            EventSystem.Instance.onGameOver += ShowRestartPanel;
            EventSystem.Instance.OnRestart += CloseRestartPanel;
            EventSystem.Instance.onAbilityCooldown += ResetCooldown;
            EventSystem.Instance.resetAbilityCooldown += ResetCooldownImmediately;
            EventSystem.Instance.OnRestart += ResetHP;

            Debug.Log("UI_Controller подписался на EventSystem");
        }

        InitializePlayerHealth();

        InitializeCooldownMaterial();
    }

    private void OnDisable()
    {
        EventSystem.Instance.onGameOver -= ShowRestartPanel;
        EventSystem.Instance.OnRestart -= CloseRestartPanel;
        EventSystem.Instance.onAbilityCooldown -= ResetCooldown;
        EventSystem.Instance.resetAbilityCooldown -= ResetCooldownImmediately;
        EventSystem.Instance.OnRestart -= ResetHP;


        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= UpdatePlayerHealthBar;
        }

        if (cooldownCoroutine != null)
        {
            StopCoroutine(cooldownCoroutine);
            cooldownCoroutine = null;
        }
    }

    private void OnDestroy()
    {
        // Очищаем созданный материал
        if (cooldownMaterial != null)
        {
            Destroy(cooldownMaterial);
            cooldownMaterial = null;
        }
    }


    private void ShowRestartPanel() => RestartPanel.SetActive(true);
    private void CloseRestartPanel() => RestartPanel.SetActive(false);


    private void ResetHP()
    {
        healthText.text = $"XP: 100";
        UpdatePlayerHealthBar(100, 100);
    }

    private void InitializePlayerHealth()
    {
        // Находим игрока по тегу
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            playerHealth = playerObj.GetComponent<IHealth>();

        if (playerHealth == null)
        {
            Debug.LogError("UI_Controller: Player not found or missing IHealth!");
            return;
        }

        // Клонируем материал, чтобы не изменять исходный ассет
        if (playerHealthImage != null)
        {
            playerHealthMaterial = Instantiate(playerHealthImage.material);
            playerHealthImage.material = playerHealthMaterial;
        }

        playerHealth.OnHealthChanged += UpdatePlayerHealthBar;

        UpdatePlayerHealthBar(playerHealth.CurrentHealth, playerHealth.MaxHealth);
    }

    private void UpdatePlayerHealthBar(float current, float max)
    {
        if (playerHealthMaterial != null)
        {
            float fill = current / max;
            playerHealthMaterial.SetFloat(healthFillProperty, fill);
        }

        healthText.text = $"XP: {current}";
    }

    // Запустить кулдаун
    private void ResetCooldown(int seconds, float elapsed = 0f)
    {
        if (cooldownCoroutine != null)
            StopCoroutine(cooldownCoroutine);

        float _elapsed = Mathf.Max(0, elapsed);
        cooldownCoroutine = StartCoroutine(CooldownAnimation(seconds, _elapsed));
    }

    // Сбросить кулдаун 
    private void ResetCooldownImmediately()
    {
        if (cooldownCoroutine != null)
        {
            StopCoroutine(cooldownCoroutine);
            cooldownCoroutine = null;
        }

        if (cooldownMaterial != null)
            cooldownMaterial.SetFloat(progressProperty, 1f);
    }

    private void InitializeCooldownMaterial()
    {
        if (cooldownImage != null)
        {
            cooldownMaterial = Instantiate(cooldownImage.material);
            cooldownImage.material = cooldownMaterial;

            cooldownMaterial.SetFloat(progressProperty, 1f);
        }
    }


    private IEnumerator CooldownAnimation(int totalSeconds, float elapsed = 0f)
    {
        float _elapsed = Mathf.Min(elapsed, totalSeconds);
        float elapsedTime = Mathf.Max(0, _elapsed);

        if (cooldownMaterial == null && cooldownImage != null)
            InitializeCooldownMaterial();

        if (cooldownMaterial == null)
            yield break;

        while (elapsedTime < totalSeconds)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / totalSeconds;
            cooldownMaterial.SetFloat(progressProperty, progress);

            yield return null;
        }

        cooldownMaterial.SetFloat(progressProperty, 1f);

        cooldownCoroutine = null;
    }
}
