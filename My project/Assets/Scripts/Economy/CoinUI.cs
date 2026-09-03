using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CoinUI : MonoBehaviour
{
    [Header("Componentes de Texto (atribua um dos dois)")]
    [SerializeField] private TextMeshProUGUI tmpText;
    [SerializeField] private Text uiText;

    [Header("Formatação")]
    [SerializeField] private string prefix = "Moedas: ";

    private void Awake()
    {
        if (tmpText == null) tmpText = GetComponent<TextMeshProUGUI>();
        if (uiText == null) uiText = GetComponent<Text>();
    }

    private void OnEnable()
    {
        CoinManager.OnCoinsChanged += UpdateCoinDisplay;
        if (CoinManager.Instance != null)
        {
            UpdateCoinDisplay(CoinManager.Instance.CurrentCoins);
        }
    }

    private void OnDisable()
    {
        CoinManager.OnCoinsChanged -= UpdateCoinDisplay;
    }

    private void UpdateCoinDisplay(int newCoinAmount)
    {
        string formattedText = $"{prefix}{newCoinAmount}";

        if (tmpText != null)
        {
            tmpText.text = formattedText;
        }
        else if (uiText != null)
        {
            uiText.text = formattedText;
        }
    }
}
