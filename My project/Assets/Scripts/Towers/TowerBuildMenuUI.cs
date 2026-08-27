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
        new TowerOption { towerName = "Metralhadora Rápida", cost = 100 },
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

        // Auto-busca botões filhos (Tower1, Tower2, Tower3, Close) se não tiverem sido arrastados
        AutoFindButtons();
    }

    private void AutoFindButtons()
    {
        Transform t1 = transform.Find("Tower1");
        if (t1 != null && towerOptions.Length > 0 && towerOptions[0].buildButton == null)
        {
            towerOptions[0].buildButton = t1.GetComponent<Button>();
            towerOptions[0].buttonLabelTMP = t1.GetComponentInChildren<TextMeshProUGUI>();
        }

        Transform t2 = transform.Find("Tower2");
        if (t2 != null && towerOptions.Length > 1 && towerOptions[1].buildButton == null)
        {
            towerOptions[1].buildButton = t2.GetComponent<Button>();
            towerOptions[1].buttonLabelTMP = t2.GetComponentInChildren<TextMeshProUGUI>();
        }

        Transform t3 = transform.Find("Tower3");
        if (t3 != null && towerOptions.Length > 2 && towerOptions[2].buildButton == null)
        {
            towerOptions[2].buildButton = t3.GetComponent<Button>();
            towerOptions[2].buttonLabelTMP = t3.GetComponentInChildren<TextMeshProUGUI>();
        }

        Transform closeT = transform.Find("Close");
        if (closeT != null && closeButton == null)
        {
            closeButton = closeT.GetComponent<Button>();
        }
    }

    private void Start()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseMenu);
        }

        // Configura listeners para cada botão de torre
        for (int i = 0; i < towerOptions.Length; i++)
        {
            int index = i;
            if (towerOptions[i].buildButton != null)
            {
                towerOptions[i].buildButton.onClick.AddListener(() => OnBuildButtonClicked(index));
            }
        }

        CoinManager.OnCoinsChanged += OnCoinsChanged;
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

            if (opt.buttonLabelTMP != null) opt.buttonLabelTMP.text = label;
            if (opt.buttonLabelLegacyText != null) opt.buttonLabelLegacyText.text = label;

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
