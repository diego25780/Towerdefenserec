using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BaseHealthUI : MonoBehaviour
{
    [Header("Componentes de UI")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI healthTMP;
    [SerializeField] private Text healthLegacyText;

    [Header("Formatação")]
    [SerializeField] private string prefix = "Vida da Base: ";

    private void Awake()
    {
        if (healthSlider == null) healthSlider = GetComponent<Slider>();
        if (healthTMP == null) healthTMP = GetComponent<TextMeshProUGUI>();
        if (healthLegacyText == null) healthLegacyText = GetComponent<Text>();
    }

    private void OnEnable()
    {
        PlayerBase.OnBaseHealthChanged += UpdateBaseHealth;
        if (PlayerBase.Instance != null)
        {
            UpdateBaseHealth(PlayerBase.Instance.CurrentHealth, PlayerBase.Instance.MaxHealth);
        }
    }

    private void OnDisable()
    {
        PlayerBase.OnBaseHealthChanged -= UpdateBaseHealth;
    }

    private void UpdateBaseHealth(int currentHealth, int maxHealth)
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        string text = $"{prefix}{currentHealth} / {maxHealth}";

        if (healthTMP != null)
        {
            healthTMP.text = text;
        }
        else if (healthLegacyText != null)
        {
            healthLegacyText.text = text;
        }
    }
}
