using System;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public static readonly List<Enemy> ActiveEnemies = new List<Enemy>();

    [Header("Configurações de Vida")]
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    [Header("Recompensa / Efeitos")]
    [Tooltip("Quantidade de moedas que este inimigo concede ao morrer.")]
    [SerializeField] private int goldReward = 10;
    [SerializeField] private GameObject deathEffect;

    public static event Action<Enemy> OnEnemySpawned;
    public static event Action<Enemy> OnEnemyDied;
    public event Action<float, float> OnHealthChanged; // (currentHealth, maxHealth)

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public int GoldReward => goldReward;
    public bool IsDead => currentHealth <= 0;

    private void Awake()
    {
        ActiveEnemies.Add(this);
    }

    private void Start()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnEnemySpawned?.Invoke(this);
    }

    private void OnDestroy()
    {
        ActiveEnemies.Remove(this);
    }

    public void TakeDamage(float damage)
    {
        if (IsDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // Concede moedas ao jogador
        if (CoinManager.Instance != null && goldReward > 0)
        {
            CoinManager.Instance.AddCoins(goldReward);
        }

        OnEnemyDied?.Invoke(this);

        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}
