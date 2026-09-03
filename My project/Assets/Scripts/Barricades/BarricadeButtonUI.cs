using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BarricadeButtonUI : MonoBehaviour
{
    [Header("Configurações do Botão")]
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI buttonTMP;
    [SerializeField] private Text buttonLegacyText;
    [SerializeField] private int barricadeCost = 40;

    private void Awake()
    {
        if (button == null) button = GetComponent<Button>();
        if (buttonTMP == null) buttonTMP = GetComponentInChildren<TextMeshProUGUI>();
        if (buttonLegacyText == null) buttonLegacyText = GetComponentInChildren<Text>();

        // Configura cores de destaque visíveis no botão (hover amarelo suave, pressionado cinza)
        if (button != null)
        {
            ColorBlock cb = button.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = new Color(1f, 0.95f, 0.6f, 1f); // Amarelo suave no hover
            cb.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            cb.disabledColor = new Color(0.7f, 0.7f, 0.7f, 0.5f);
            button.colors = cb;

            button.onClick.AddListener(OnClick);
        }
    }

    private void Start()
    {
        CoinManager.OnCoinsChanged += OnCoinsChanged;
        UpdateDisplay();
    }

    private void OnDestroy()
    {
        CoinManager.OnCoinsChanged -= OnCoinsChanged;
    }

    private void OnCoinsChanged(int coins)
    {
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        int coins = CoinManager.Instance != null ? CoinManager.Instance.CurrentCoins : 0;
        int cost = BarricadePlacement.Instance != null ? BarricadePlacement.Instance.BarricadeCost : barricadeCost;

        string label = $"Barreira ({cost}$)";
        if (buttonTMP != null) buttonTMP.text = label;
        if (buttonLegacyText != null) buttonLegacyText.text = label;

        if (button != null)
        {
            button.interactable = coins >= cost;
        }
    }

    private void OnClick()
    {
        if (BarricadePlacement.Instance != null)
        {
            BarricadePlacement.Instance.StartPlacement();
        }
    }
}
