using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GuardUpgradeUI : MonoBehaviour
{
    [Header("Referência da Barraca")]
    [Tooltip("Deixe vazio para conectar automaticamente à última barraca selecionada.")]
    [SerializeField] private GuardBarracks targetBarracks;

    [Header("Botões de Evolução")]
    [SerializeField] private Button damageUpgradeButton;
    [SerializeField] private Button healthUpgradeButton;

    [Header("Textos Informativos (TextMeshProUGUI ou Text)")]
    [SerializeField] private TextMeshProUGUI damageCostTMP;
    [SerializeField] private TextMeshProUGUI healthCostTMP;
    [SerializeField] private TextMeshProUGUI levelsTMP;

    [SerializeField] private Text damageCostLegacyText;
    [SerializeField] private Text healthCostLegacyText;
    [SerializeField] private Text levelsLegacyText;

    private void Start()
    {
        if (targetBarracks == null)
        {
            targetBarracks = GuardBarracks.SelectedBarracks;
        }

        if (damageUpgradeButton != null)
        {
            damageUpgradeButton.onClick.AddListener(OnDamageUpgradeClicked);
        }

        if (healthUpgradeButton != null)
        {
            healthUpgradeButton.onClick.AddListener(OnHealthUpgradeClicked);
        }

        GuardBarracks.OnBarracksSelected += SetTargetBarracks;
        CoinManager.OnCoinsChanged += OnCoinsChanged;

        UpdateUI();
    }

    private void OnDestroy()
    {
        GuardBarracks.OnBarracksSelected -= SetTargetBarracks;
        CoinManager.OnCoinsChanged -= OnCoinsChanged;
    }

    public void SetTargetBarracks(GuardBarracks barracks)
    {
        if (targetBarracks != null)
        {
            targetBarracks.OnUpgradesChanged -= UpdateUI;
        }

        targetBarracks = barracks;

        if (targetBarracks != null)
        {
            targetBarracks.OnUpgradesChanged += UpdateUI;
        }

        UpdateUI();
    }

    private void OnCoinsChanged(int currentCoins)
    {
        UpdateUI();
    }

    public void OnDamageUpgradeClicked()
    {
        if (targetBarracks != null)
        {
            targetBarracks.UpgradeDamage();
            UpdateUI();
        }
    }

    public void OnHealthUpgradeClicked()
    {
        if (targetBarracks != null)
        {
            targetBarracks.UpgradeHealth();
            UpdateUI();
        }
    }

    public void UpdateUI()
    {
        if (targetBarracks == null) return;

        int dmgCost = targetBarracks.CurrentDamageUpgradeCost;
        int hpCost = targetBarracks.CurrentHealthUpgradeCost;
        int currentCoins = CoinManager.Instance != null ? CoinManager.Instance.CurrentCoins : 0;

        string dmgText = $"Evoluir Dano (Nv {targetBarracks.DamageLevel})\nCusto: {dmgCost} Moedas";
        string hpText = $"Evoluir Vida (Nv {targetBarracks.HealthLevel})\nCusto: {hpCost} Moedas";
        string lvlText = $"Dano: Nv {targetBarracks.DamageLevel} | Vida: Nv {targetBarracks.HealthLevel}";

        if (damageCostTMP != null) damageCostTMP.text = dmgText;
        if (healthCostTMP != null) healthCostTMP.text = hpText;
        if (levelsTMP != null) levelsTMP.text = lvlText;

        if (damageCostLegacyText != null) damageCostLegacyText.text = dmgText;
        if (healthCostLegacyText != null) healthCostLegacyText.text = hpText;
        if (levelsLegacyText != null) levelsLegacyText.text = lvlText;

        if (damageUpgradeButton != null)
        {
            damageUpgradeButton.interactable = currentCoins >= dmgCost;
        }

        if (healthUpgradeButton != null)
        {
            healthUpgradeButton.interactable = currentCoins >= hpCost;
        }
    }
}
