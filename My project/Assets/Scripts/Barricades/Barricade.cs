using System;
using UnityEngine;

public class Barricade : MonoBehaviour
{
    [Header("Configurações da Barreira")]
    [SerializeField] private float maxHealth = 300f;
    [SerializeField] private GameObject breakEffect;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private float currentHealth;
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsDestroyed => currentHealth <= 0;

    public event Action<float, float> OnHealthChanged;
    public static event Action<Barricade> OnBarricadeDestroyed;

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (spriteRenderer != null && spriteRenderer.sortingOrder < 8)
        {
            spriteRenderer.sortingOrder = 8;
        }
    }

    private void Start()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void TakeDamage(float damage)
    {
        if (IsDestroyed) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        // Feedback visual de dano (escurece levemente o sprite conforme a vida diminui)
        if (spriteRenderer != null)
        {
            float healthRatio = currentHealth / maxHealth;
            spriteRenderer.color = Color.Lerp(new Color(0.5f, 0.2f, 0.2f), Color.white, healthRatio);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        OnBarricadeDestroyed?.Invoke(this);

        if (breakEffect != null)
        {
            Instantiate(breakEffect, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}
