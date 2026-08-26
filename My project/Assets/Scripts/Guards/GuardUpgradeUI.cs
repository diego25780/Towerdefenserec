using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GuardUpgradeUI : MonoBehaviour
{
    [Header("Referência da Barraca")]
    [Tooltip("Deixe vazio para conectar automaticamente à última barraca selecionada.")]
    [SerializeField] private GuardBarracks targetBarracks;

    [Header("Botões de Ação")]
    [SerializeField] private Button buyGuardButton;
    [SerializeField] private Button damageUpgradeButton;
    [SerializeField] private Button healthUpgradeButton;

    [Header("Textos Informativos (TextMeshProUGUI ou Text)")]
    [SerializeField] private TextMeshProUGUI buyGuardTMP;
    [SerializeField] private TextMeshProUGUI damageCostTMP;
    [SerializeField] private TextMeshProUGUI healthCostTMP;
    [SerializeField] private TextMeshProUGUI levelsTMP;
    [SerializeField] private TextMeshProUGUI guardCountTMP;

    [SerializeField] private Text buyGuardLegacyText;
    [SerializeField] private Text damageCostLegacyText;
    [SerializeField] private Text healthCostLegacyText;
    [SerializeField] private Text levelsLegacyText;
    [SerializeField] private Text guardCountLegacyText;

    private void Start()
    {
        if (targetBarracks == null)
        {
            targetBarracks = GuardBarracks.SelectedBarracks;
        }

        if (buyGuardButton != null)
        {
            buyGuardButton.onClick.AddListener(OnBuyGuardClicked);
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

        if (targetBarracks != null)
        {
            targetBarracks.OnUpgradesChanged += UpdateUI;
            targetBarracks.OnGuardsChanged += UpdateUI;
        }

        UpdateUI();
    }

    private void OnDestroy()
    {
        GuardBarracks.OnBarracksSelected -= SetTargetBarracks;
        CoinManager.OnCoinsChanged -= OnCoinsChanged;
        if (targetBarracks != null)
        {
            targetBarracks.OnUpgradesChanged -= UpdateUI;
            targetBarracks.OnGuardsChanged -= UpdateUI;
        }
    }

    public void SetTargetBarracks(GuardBarracks barracks)
    {
        if (targetBarracks != null)
        {
            targetBarracks.OnUpgradesChanged -= UpdateUI;
            targetBarracks.OnGuardsChanged -= UpdateUI;
        }

        targetBarracks = barracks;

        if (targetBarracks != null)
        {
            targetBarracks.OnUpgradesChanged += UpdateUI;
            targetBarracks.OnGuardsChanged += UpdateUI;
        }

        UpdateUI();
    }

    private void OnCoinsChanged(int currentCoins)
    {
        UpdateUI();
    }

    public void OnBuyGuardClicked()
    {
        if (targetBarracks != null)
        {
            targetBarracks.BuyExtraGuard();
            UpdateUI();
        }
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

        int buyCost = targetBarracks.BuyGuardCost;
        int dmgCost = targetBarracks.CurrentDamageUpgradeCost;
        int hpCost = targetBarracks.CurrentHealthUpgradeCost;
        int currentCoins = CoinManager.Instance != null ? CoinManager.Instance.CurrentCoins : 0;

        string buyText = targetBarracks.CanBuyMoreGuards 
            ? $"Recrutar Guarda\nCusto: {buyCost} Moedas" 
            : "Limite Máximo Atingido";

        string dmgText = $"Evoluir Dano (Nv {targetBarracks.DamageLevel})\nCusto: {dmgCost} Moedas";
        string hpText = $"Evoluir Vida (Nv {targetBarracks.HealthLevel})\nCusto: {hpCost} Moedas";
        string lvlText = $"Dano: Nv {targetBarracks.DamageLevel} | Vida: Nv {targetBarracks.HealthLevel}";
        string countText = $"Guardas: {targetBarracks.CurrentGuardLimit} / {targetBarracks.MaxGuardCount}";

        if (buyGuardTMP != null) buyGuardTMP.text = buyText;
        if (damageCostTMP != null) damageCostTMP.text = dmgText;
        if (healthCostTMP != null) healthCostTMP.text = hpText;
        if (levelsTMP != null) levelsTMP.text = lvlText;
        if (guardCountTMP != null) guardCountTMP.text = countText;

        if (buyGuardLegacyText != null) buyGuardLegacyText.text = buyText;
        if (damageCostLegacyText != null) damageCostLegacyText.text = dmgText;
        if (healthCostLegacyText != null) healthCostLegacyText.text = hpText;
        if (levelsLegacyText != null) levelsLegacyText.text = lvlText;
        if (guardCountLegacyText != null) guardCountLegacyText.text = countText;

        if (buyGuardButton != null)
        {
            buyGuardButton.interactable = targetBarracks.CanBuyMoreGuards && currentCoins >= buyCost;
        }

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
