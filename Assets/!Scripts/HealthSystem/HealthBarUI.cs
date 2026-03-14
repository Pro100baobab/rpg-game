using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private string fillProperty = "_FillAmount";
    private Material healthBarMaterial;
    private IHealth health;

    private void Start()
    {
        health = GetComponentInParent<IHealth>();
        if (health == null)
        {
            Debug.LogError("HealthBarUI: Нет компонента IHealth на родителе!");
            return;
        }

        var image = GetComponent<Image>();
        if (image != null)
        {
            healthBarMaterial = Instantiate(image.material);
            image.material = healthBarMaterial;
        }

        health.OnHealthChanged += UpdateHealthBar;
        health.OnDeath += OnDeath;

        UpdateHealthBar(health.CurrentHealth, health.MaxHealth);
    }

    private void UpdateHealthBar(float current, float max)
    {
        if (healthBarMaterial != null)
        {
            float fill = current / max;
            healthBarMaterial.SetFloat(fillProperty, fill);
        }
    }

    private void OnDeath()
    {
        if (healthBarMaterial != null)
            healthBarMaterial.SetFloat(fillProperty, 0f);

        // Отписываться необязательно, объект всё равно будет деактивирован или перезапущен
    }

    private void OnDestroy()
    {
        if (health != null)
        {
            health.OnHealthChanged -= UpdateHealthBar;
            health.OnDeath -= OnDeath;
        }
        if (healthBarMaterial != null)
            Destroy(healthBarMaterial);
    }
}