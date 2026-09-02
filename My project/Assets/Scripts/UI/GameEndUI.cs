using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameEndUI : MonoBehaviour
{
    [Header("Painel de Vitória")]
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private Button victoryRestartButton;
    [SerializeField] private TextMeshProUGUI victoryTitleTMP;

    [Header("Painel de Derrota")]
    [SerializeField] private GameObject defeatPanel;
    [SerializeField] private Button defeatRestartButton;
    [SerializeField] private TextMeshProUGUI defeatTitleTMP;

    private void Start()
    {
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (defeatPanel != null) defeatPanel.SetActive(false);

        if (victoryRestartButton != null)
        {
            victoryRestartButton.onClick.AddListener(OnRestartClicked);
        }

        if (defeatRestartButton != null)
        {
            defeatRestartButton.onClick.AddListener(OnRestartClicked);
        }

        GameManager.OnGameWon += ShowVictory;
        GameManager.OnGameLost += ShowDefeat;
    }

    private void OnDestroy()
    {
        GameManager.OnGameWon -= ShowVictory;
        GameManager.OnGameLost -= ShowDefeat;
    }

    private void ShowVictory()
    {
        if (defeatPanel != null) defeatPanel.SetActive(false);
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }
        if (victoryTitleTMP != null)
        {
            victoryTitleTMP.text = "🎉 VITÓRIA!";
        }
    }

    private void ShowDefeat()
    {
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (defeatPanel != null)
        {
            defeatPanel.SetActive(true);
        }
        if (defeatTitleTMP != null)
        {
            defeatTitleTMP.text = "💀 DERROTA!";
        }
    }

    public void OnRestartClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
        else
        {
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
    }
}
