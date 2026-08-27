using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TowerUpgradeUI : MonoBehaviour
{
    public static TowerUpgradeUI Instance { get; private set; }

    [Header("Referência da Torre Selecionada")]
    [SerializeField] private Tower selectedTower;

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

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        if (damageUpgradeButton != null)
        {
            damageUpgradeButton.onClick.AddListener(OnDamageUpgradeClicked);
        }

        if (rangeUpgradeButton != null)
        {
            rangeUpgradeButton.onClick.AddListener(OnRangeUpgradeClicked);
        }

        Tower.OnTowerSelected += SetSelectedTower;
        CoinManager.OnCoinsChanged += OnCoinsChanged;

        if (selectedTower == null)
        {
            selectedTower = Tower.SelectedTower;
        }

        if (selectedTower != null)
        {
            selectedTower.OnTowerUpgraded += UpdateUI;
        }

        UpdateUI();
    }

    private void OnDestroy()
    {
        Tower.OnTowerSelected -= SetSelectedTower;
        CoinManager.OnCoinsChanged -= OnCoinsChanged;
        if (selectedTower != null)
        {
            selectedTower.OnTowerUpgraded -= UpdateUI;
        }
    }

    public void SetSelectedTower(Tower tower)
    {
        if (selectedTower != null)
        {
            selectedTower.OnTowerUpgraded -= UpdateUI;
        }

        selectedTower = tower;

        if (selectedTower != null)
        {
            selectedTower.OnTowerUpgraded += UpdateUI;
        }

        UpdateUI();
    }

    private void OnCoinsChanged(int currentCoins)
    {
        UpdateUI();
    }

    public void OnDamageUpgradeClicked()
    {
        if (selectedTower != null)
        {
            selectedTower.UpgradeDamage();
            UpdateUI();
        }
    }

    public void OnRangeUpgradeClicked()
    {
        if (selectedTower != null)
        {
            selectedTower.UpgradeRange();
            UpdateUI();
        }
    }

    public void UpdateUI()
    {
        if (selectedTower == null)
        {
            string noTower = "Selecione uma Torre no Mapa";
            if (towerNameTMP != null) towerNameTMP.text = noTower;
            if (damageCostTMP != null) damageCostTMP.text = "-";
            if (rangeCostTMP != null) rangeCostTMP.text = "-";
            if (levelsTMP != null) levelsTMP.text = "Nenhuma torre selecionada";

            if (damageUpgradeButton != null) damageUpgradeButton.interactable = false;
            if (rangeUpgradeButton != null) rangeUpgradeButton.interactable = false;
            return;
        }

        int dmgCost = selectedTower.CurrentDamageUpgradeCost;
        int rngCost = selectedTower.CurrentRangeUpgradeCost;
        int currentCoins = CoinManager.Instance != null ? CoinManager.Instance.CurrentCoins : 0;

        string nameText = $"Torre: {selectedTower.TowerName}";
        string dmgText = $"Evoluir Dano (Nv {selectedTower.DamageLevel})\nCusto: {dmgCost}$";
        string rngText = $"Evoluir Alcance (Nv {selectedTower.RangeLevel})\nCusto: {rngCost}$";
        string lvlText = $"Dano: {selectedTower.Damage} (Nv {selectedTower.DamageLevel}) | Alcance: {selectedTower.Range:F1} (Nv {selectedTower.RangeLevel})";

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
