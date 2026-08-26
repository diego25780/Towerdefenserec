using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TowerUpgradeUI : MonoBehaviour
{
    [Header("Referência da Torre")]
    [Tooltip("Deixe vazio para conectar automaticamente à última torre selecionada.")]
    [SerializeField] private Tower targetTower;

    [Header("Botões de Evolução")]
    [SerializeField] private Button damageUpgradeButton;
    [SerializeField] private Button rangeUpgradeButton;

    [Header("Textos Informativos (TextMeshProUGUI ou Text)")]
    [SerializeField] private TextMeshProUGUI towerNameTMP;
    [SerializeField] private TextMeshProUGUI damageCostTMP;
    [SerializeField] private TextMeshProUGUI rangeCostTMP;
    [SerializeField] private TextMeshProUGUI levelsTMP;

    [SerializeField] private Text towerNameLegacyText;
    [SerializeField] private Text damageCostLegacyText;
    [SerializeField] private Text rangeCostLegacyText;
    [SerializeField] private Text levelsLegacyText;

    private void Start()
    {
        if (targetTower == null)
        {
            targetTower = Tower.SelectedTower;
        }

        if (damageUpgradeButton != null)
        {
            damageUpgradeButton.onClick.AddListener(OnDamageUpgradeClicked);
        }

        if (rangeUpgradeButton != null)
        {
            rangeUpgradeButton.onClick.AddListener(OnRangeUpgradeClicked);
        }

        Tower.OnTowerSelected += SetTargetTower;
        CoinManager.OnCoinsChanged += OnCoinsChanged;

        if (targetTower != null)
        {
            targetTower.OnTowerUpgraded += UpdateUI;
        }

        UpdateUI();
    }

    private void OnDestroy()
    {
        Tower.OnTowerSelected -= SetTargetTower;
        CoinManager.OnCoinsChanged -= OnCoinsChanged;
        if (targetTower != null)
        {
            targetTower.OnTowerUpgraded -= UpdateUI;
        }
    }

    public void SetTargetTower(Tower tower)
    {
        if (targetTower != null)
        {
            targetTower.OnTowerUpgraded -= UpdateUI;
        }

        targetTower = tower;

        if (targetTower != null)
        {
            targetTower.OnTowerUpgraded += UpdateUI;
        }

        UpdateUI();
    }

    private void OnCoinsChanged(int currentCoins)
    {
        UpdateUI();
    }

    public void OnDamageUpgradeClicked()
    {
        if (targetTower != null)
        {
            targetTower.UpgradeDamage();
            UpdateUI();
        }
    }

    public void OnRangeUpgradeClicked()
    {
        if (targetTower != null)
        {
            targetTower.UpgradeRange();
            UpdateUI();
        }
    }

    public void UpdateUI()
    {
        if (targetTower == null) return;

        int dmgCost = targetTower.CurrentDamageUpgradeCost;
        int rngCost = targetTower.CurrentRangeUpgradeCost;
        int currentCoins = CoinManager.Instance != null ? CoinManager.Instance.CurrentCoins : 0;

        string nameText = targetTower.TowerName;
        string dmgText = $"Evoluir Dano (Nv {targetTower.DamageLevel})\nCusto: {dmgCost} Moedas";
        string rngText = $"Evoluir Alcance (Nv {targetTower.RangeLevel})\nCusto: {rngCost} Moedas";
        string lvlText = $"Dano: {targetTower.Damage} (Nv {targetTower.DamageLevel}) | Alcance: {targetTower.Range:F1} (Nv {targetTower.RangeLevel})";

        if (towerNameTMP != null) towerNameTMP.text = nameText;
        if (damageCostTMP != null) damageCostTMP.text = dmgText;
        if (rangeCostTMP != null) rangeCostTMP.text = rngText;
        if (levelsTMP != null) levelsTMP.text = lvlText;

        if (towerNameLegacyText != null) towerNameLegacyText.text = nameText;
        if (damageCostLegacyText != null) damageCostLegacyText.text = dmgText;
        if (rangeCostLegacyText != null) rangeCostLegacyText.text = rngText;
        if (levelsLegacyText != null) levelsLegacyText.text = lvlText;

        if (damageUpgradeButton != null)
        {
            damageUpgradeButton.interactable = currentCoins >= dmgCost;
        }

        if (rangeUpgradeButton != null)
        {
            rangeUpgradeButton.interactable = currentCoins >= rngCost;
        }
    }
}
