using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TowerBuildMenuUI : MonoBehaviour
{
    public static TowerBuildMenuUI Instance { get; private set; }

    [System.Serializable]
    public class TowerOption
    {
        public string towerName = "Torre Padrão";
        public GameObject towerPrefab;
        public int cost = 70;
        public Button buildButton;
        public TextMeshProUGUI buttonLabelTMP;
        public Text buttonLabelLegacyText;
    }

    [Header("Painel do Menu")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private Button closeButton;

    [Header("Opções de Torres (3 Tipos)")]
    [SerializeField] private TowerOption[] towerOptions = new TowerOption[3]
    {
        new TowerOption { towerName = "Canhão Padrão", cost = 70 },
        new TowerOption { towerName = "Metralhadora", cost = 100 },
        new TowerOption { towerName = "Torre Sniper", cost = 130 }
    };

    private TowerSpot currentSpot;

    public bool IsOpen => menuPanel != null && menuPanel.activeSelf;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (menuPanel == null) menuPanel = gameObject;

        // Auto-busca botões filhos (Tower1, Tower2, Tower3, Close)
        AutoFindButtons();
    }

    private void AutoFindButtons()
    {
        Button[] childButtons = GetComponentsInChildren<Button>(true);

        // Procura botões por nome primeiro
        for (int i = 0; i < towerOptions.Length; i++)
        {
            string targetName = $"Tower{i + 1}";
            foreach (Button btn in childButtons)
            {
                if (btn.gameObject.name.Equals(targetName, System.StringComparison.OrdinalIgnoreCase))
                {
                    towerOptions[i].buildButton = btn;
                    towerOptions[i].buttonLabelTMP = btn.GetComponentInChildren<TextMeshProUGUI>(true);
                    towerOptions[i].buttonLabelLegacyText = btn.GetComponentInChildren<Text>(true);
                    break;
                }
            }
        }

        // Se algum botão não foi encontrado por nome, pega por índice
        int towerIndex = 0;
        foreach (Button btn in childButtons)
        {
            if (btn.gameObject.name.Equals("Close", System.StringComparison.OrdinalIgnoreCase))
            {
                if (closeButton == null) closeButton = btn;
                continue;
            }

            if (towerIndex < towerOptions.Length && towerOptions[towerIndex].buildButton == null)
            {
                towerOptions[towerIndex].buildButton = btn;
                towerOptions[towerIndex].buttonLabelTMP = btn.GetComponentInChildren<TextMeshProUGUI>(true);
                towerOptions[towerIndex].buttonLabelLegacyText = btn.GetComponentInChildren<Text>(true);
                towerIndex++;
            }
        }

        if (closeButton == null)
        {
            foreach (Button btn in childButtons)
            {
                if (btn.gameObject.name.IndexOf("Close", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                    btn.gameObject.name.IndexOf("Fechar", System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    closeButton = btn;
                    break;
                }
            }
        }
    }

    private void Start()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(CloseMenu);

            TextMeshProUGUI closeTMP = closeButton.GetComponentInChildren<TextMeshProUGUI>(true);
            if (closeTMP != null && closeTMP.text.Equals("Button", System.StringComparison.OrdinalIgnoreCase))
            {
                closeTMP.text = "Fechar";
            }
        }

        // Configura listeners para cada botão de torre
        for (int i = 0; i < towerOptions.Length; i++)
        {
            int index = i;
            if (towerOptions[i].buildButton != null)
            {
                towerOptions[i].buildButton.onClick.RemoveAllListeners();
                towerOptions[i].buildButton.onClick.AddListener(() => OnBuildButtonClicked(index));
            }
        }

        CoinManager.OnCoinsChanged += OnCoinsChanged;
        
        // Atualiza textos logo no início
        UpdateButtons();
        CloseMenu();
    }

    private void OnDestroy()
    {
        CoinManager.OnCoinsChanged -= OnCoinsChanged;
    }

    public void OpenMenu(TowerSpot spot)
    {
        currentSpot = spot;

        if (menuPanel != null)
        {
            menuPanel.SetActive(true);
        }

        UpdateButtons();
        Debug.Log("[TowerBuildMenuUI] Menu de construção aberto!");
    }

    public void CloseMenu()
    {
        currentSpot = null;
        if (menuPanel != null)
        {
            menuPanel.SetActive(false);
        }
    }

    private void OnCoinsChanged(int coins)
    {
        if (IsOpen)
        {
            UpdateButtons();
        }
    }

    private void UpdateButtons()
    {
        int currentCoins = CoinManager.Instance != null ? CoinManager.Instance.CurrentCoins : 0;

        for (int i = 0; i < towerOptions.Length; i++)
        {
            TowerOption opt = towerOptions[i];
            string label = $"{opt.towerName}\n({opt.cost}$)";

            // Se o botão não estiver preenchido, tenta buscar novamente
            if (opt.buttonLabelTMP == null && opt.buildButton != null)
            {
                opt.buttonLabelTMP = opt.buildButton.GetComponentInChildren<TextMeshProUGUI>(true);
            }
            if (opt.buttonLabelLegacyText == null && opt.buildButton != null)
            {
                opt.buttonLabelLegacyText = opt.buildButton.GetComponentInChildren<Text>(true);
            }

            if (opt.buttonLabelTMP != null)
            {
                opt.buttonLabelTMP.text = label;
            }
            if (opt.buttonLabelLegacyText != null)
            {
                opt.buttonLabelLegacyText.text = label;
            }

            if (opt.buildButton != null)
            {
                opt.buildButton.interactable = currentCoins >= opt.cost;
            }
        }
    }

    private void OnBuildButtonClicked(int towerIndex)
    {
        if (currentSpot == null || towerIndex < 0 || towerIndex >= towerOptions.Length) return;

        TowerOption selectedOption = towerOptions[towerIndex];

        if (selectedOption.towerPrefab == null)
        {
            Debug.LogWarning($"[TowerBuildMenuUI] O Prefab da '{selectedOption.towerName}' não foi colocado no Inspector do TowerBuildUi!");
            return;
        }

        bool success = currentSpot.BuildTower(selectedOption.towerPrefab, selectedOption.cost);
        if (success)
        {
            CloseMenu();
        }
    }
}
