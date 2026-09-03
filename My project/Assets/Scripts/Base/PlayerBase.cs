using System;
using UnityEngine;

public class PlayerBase : MonoBehaviour
{
    public static PlayerBase Instance { get; private set; }

    [Header("Configurações da Base")]
    [SerializeField] private int maxHealth = 20;
    private int currentHealth;

    public static event Action<int, int> OnBaseHealthChanged; // (currentHealth, maxHealth)
    public static event Action OnBaseDestroyed;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        currentHealth = maxHealth;
        OnBaseHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        OnBaseHealthChanged?.Invoke(currentHealth, maxHealth);
        Debug.Log($"Base atingida! Vida: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Debug.Log("Game Over! A base foi destruída.");
            OnBaseDestroyed?.Invoke();
        }
    }
}
