using System;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Configurações de Vida")]
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    [Header("Recompensa / Efeitos")]
    [SerializeField] private int goldReward = 10;
    [SerializeField] private GameObject deathEffect;

    public static event Action<Enemy> OnEnemyDied;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public int GoldReward => goldReward;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        OnEnemyDied?.Invoke(this);

        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}
