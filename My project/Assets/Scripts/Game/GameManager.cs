using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Painéis de Fim de Jogo")]
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private GameObject defeatPanel;

    [Header("Configurações")]
    [SerializeField] private bool pauseGameOnEnd = true;

    public static event Action OnGameWon;
    public static event Action OnGameLost;

    public bool IsGameOver { get; private set; } = false;

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

        // Garante que o tempo está rodando normalmente ao carregar a cena
        Time.timeScale = 1f;
    }

    private void Start()
    {
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (defeatPanel != null) defeatPanel.SetActive(false);

        PlayerBase.OnBaseDestroyed += HandleBaseDestroyed;
        EnemySpawner.OnAllWavesCompleted += CheckForVictory;
        Enemy.OnEnemyDied += HandleEnemyDied;
    }

    private void OnDestroy()
    {
        PlayerBase.OnBaseDestroyed -= HandleBaseDestroyed;
        EnemySpawner.OnAllWavesCompleted -= CheckForVictory;
        Enemy.OnEnemyDied -= HandleEnemyDied;
    }

    private void HandleBaseDestroyed()
    {
        if (IsGameOver) return;
        TriggerDefeat();
    }

    private void HandleEnemyDied(Enemy deadEnemy)
    {
        if (IsGameOver) return;
        CheckForVictory();
    }

    public void CheckForVictory()
    {
        if (IsGameOver) return;

        // Se o spawner terminou todas as waves e não restou nenhum inimigo vivo
        if (EnemySpawner.Instance != null && EnemySpawner.Instance.AreAllWavesCompleted)
        {
            // Verifica inimigos ativos
            int livingEnemies = 0;
            foreach (Enemy e in Enemy.ActiveEnemies)
            {
                if (e != null && !e.IsDead) livingEnemies++;
            }

            if (livingEnemies <= 0)
            {
                TriggerVictory();
            }
        }
    }

    private void TriggerVictory()
    {
        if (IsGameOver) return;
        IsGameOver = true;

        Debug.Log("🎉 VITÓRIA! Todas as waves foram derrotadas e a base sobreviveu!");

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }

        if (pauseGameOnEnd)
        {
            Time.timeScale = 0f;
        }

        OnGameWon?.Invoke();
    }

    private void TriggerDefeat()
    {
        if (IsGameOver) return;
        IsGameOver = true;

        Debug.Log("💀 DERROTA! A sua base foi destruída!");

        if (defeatPanel != null)
        {
            defeatPanel.SetActive(true);
        }

        if (pauseGameOnEnd)
        {
            Time.timeScale = 0f;
        }

        OnGameLost?.Invoke();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
        Debug.Log("Saindo do jogo...");
    }
}
