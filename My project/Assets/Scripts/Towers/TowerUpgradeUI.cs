using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TowerUpgradeUI : MonoBehaviour
{
    public static TowerUpgradeUI Instance { get; private set; }

    [Header("Referência da Torre Selecionada")]
    [SerializeField] private Tower selectedTower;

    [Header("Botões de Ação")]
    [SerializeField] private Button damageUpgradeButton;
    [SerializeField] private Button rangeUpgradeButton;
    [SerializeField] private Button sellButton;

    [Header("Textos Informativos (TextMeshProUGUI ou Text)")]
    [SerializeField] private TextMeshProUGUI towerNameTMP;
    [SerializeField] private TextMeshProUGUI damageCostTMP;
    [SerializeField] private TextMeshProUGUI rangeCostTMP;
    [SerializeField] private TextMeshProUGUI sellCostTMP;
    [SerializeField] private TextMeshProUGUI levelsTMP;

    [SerializeField] private Text towerNameLegacyText;
    [SerializeField] private Text damageCostLegacyText;
    [SerializeField] private Text rangeCostLegacyText;
    [SerializeField] private Text sellCostLegacyText;
    [SerializeField] private Text levelsLegacyText;

    private void Awake()
    {
        if (Instance == null) Instance = this;

        // Desativa raycast em imagem de fundo transparente se houver
        Image img = GetComponent<Image>();
        if (img != null && img.color.a <= 0.05f)
        {
            img.raycastTarget = false;
        }

        AutoFindSellButton();
    }

    private void AutoFindSellButton()
    {
        if (sellButton == null)
        {
            Button[] buttons = GetComponentsInChildren<Button>(true);
            foreach (Button b in buttons)
            {
                if (b.gameObject.name.IndexOf("Sell", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                    b.gameObject.name.IndexOf("Vender", System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    sellButton = b;
                    sellCostTMP = b.GetComponentInChildren<TextMeshProUGUI>(true);
                    sellCostLegacyText = b.GetComponentInChildren<Text>(true);
                    break;
                }
            }
        }
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

        if (sellButton != null)
        {
            sellButton.onClick.AddListener(OnSellButtonClicked);
        }

        Tower.OnTowerSelected += SetSelectedTower;
        Tower.OnTowerDeselected += ClearSelectedTower;
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
        Tower.OnTowerDeselected -= ClearSelectedTower;
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

    public void ClearSelectedTower()
    {
        if (selectedTower != null)
        {
            selectedTower.OnTowerUpgraded -= UpdateUI;
        }
        selectedTower = null;
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

    public void OnSellButtonClicked()
    {
        if (selectedTower != null)
        {
            selectedTower.SellTower();
            ClearSelectedTower();
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
            if (sellCostTMP != null) sellCostTMP.text = "-";
            if (levelsTMP != null) levelsTMP.text = "Nenhuma torre selecionada";

            if (damageUpgradeButton != null) damageUpgradeButton.interactable = false;
            if (rangeUpgradeButton != null) rangeUpgradeButton.interactable = false;
            if (sellButton != null) sellButton.interactable = false;
            return;
        }

        int currentCoins = CoinManager.Instance != null ? CoinManager.Instance.CurrentCoins : 0;
        int dmgCost = selectedTower.CurrentDamageUpgradeCost;
        int rngCost = selectedTower.CurrentRangeUpgradeCost;
        int sellVal = selectedTower.SellValue;

        string nameText = $"Torre: {selectedTower.TowerName}";
        string sellText = $"Vender (+{sellVal}$)";
        
        // Texto de Dano
        string dmgText;
        bool canUpgradeDmg = selectedTower.CanUpgradeDamage;
        if (canUpgradeDmg)
        {
            dmgText = $"Evoluir Dano (Nv {selectedTower.DamageLevel}/{selectedTower.MaxDamageLevel})\nCusto: {dmgCost}$";
        }
        else
        {
            dmgText = $"Dano no MÁXIMO (Nv {selectedTower.MaxDamageLevel})";
        }

        // Texto de Alcance
        string rngText;
        bool canUpgradeRng = selectedTower.CanUpgradeRange;
        if (canUpgradeRng)
        {
            rngText = $"Evoluir Alcance (Nv {selectedTower.RangeLevel}/{selectedTower.MaxRangeLevel})\nCusto: {rngCost}$";
        }
        else
        {
            rngText = $"Alcance no MÁXIMO (Nv {selectedTower.MaxRangeLevel})";
        }

        string lvlText = $"Dano: {selectedTower.Damage} (Nv {selectedTower.DamageLevel}) | Alcance: {selectedTower.Range:F1} (Nv {selectedTower.RangeLevel})";

        if (towerNameTMP != null) towerNameTMP.text = nameText;
        if (damageCostTMP != null) damageCostTMP.text = dmgText;
        if (rangeCostTMP != null) rangeCostTMP.text = rngText;
        if (sellCostTMP != null) sellCostTMP.text = sellText;
        if (levelsTMP != null) levelsTMP.text = lvlText;

        if (towerNameLegacyText != null) towerNameLegacyText.text = nameText;
        if (damageCostLegacyText != null) damageCostLegacyText.text = dmgText;
        if (rangeCostLegacyText != null) rangeCostLegacyText.text = rngText;
        if (sellCostLegacyText != null) sellCostLegacyText.text = sellText;
        if (levelsLegacyText != null) levelsLegacyText.text = lvlText;

        if (damageUpgradeButton != null)
        {
            damageUpgradeButton.interactable = canUpgradeDmg && currentCoins >= dmgCost;
        }

        if (rangeUpgradeButton != null)
        {
            rangeUpgradeButton.interactable = canUpgradeRng && currentCoins >= rngCost;
        }

        if (sellButton != null)
        {
            sellButton.interactable = true;
        }
    }
}
