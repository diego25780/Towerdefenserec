using System.Collections;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [Header("Ataque do Inimigo")]
    [SerializeField] private float damage = 5f;
    [SerializeField] private float attackRate = 1f; // Golpes por segundo
    [SerializeField] private float attackRange = 1.0f;

    [Header("Alvos")]
    [SerializeField] private string barricadeTag = "Barricade";

    private float attackCountdown = 0f;
    private EnemyMovement enemyMovement;
    private Transform currentBarricadeTarget;
    private SpriteRenderer spriteRenderer;

    public bool IsAttacking => enemyMovement != null && (enemyMovement.HasReachedTower || currentBarricadeTarget != null);

    private void Awake()
    {
        enemyMovement = GetComponent<EnemyMovement>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        InvokeRepeating(nameof(CheckForBarricades), 0f, 0.15f);
    }

    private void Update()
    {
        if (attackCountdown > 0f)
        {
            attackCountdown -= Time.deltaTime;
        }

        // 1. Se encontrou uma barricada no caminho, ataca a barricada até quebrar
        if (currentBarricadeTarget != null)
        {
            Barricade b = currentBarricadeTarget.GetComponent<Barricade>();
            if (b == null || b.IsDestroyed)
            {
                currentBarricadeTarget = null;
                if (enemyMovement != null) enemyMovement.SetBlocked(false);
                return;
            }

            if (enemyMovement != null) enemyMovement.SetBlocked(true);

            if (attackCountdown <= 0f)
            {
                AttackBarricade(b);
                attackCountdown = 1f / attackRate;
            }
            return;
        }

        // 2. Se alcançou a Torre / Base, fica batendo nela continuamente
        if (enemyMovement != null && enemyMovement.HasReachedTower)
        {
            if (attackCountdown <= 0f)
            {
                AttackTower();
                attackCountdown = 1f / attackRate;
            }
        }
    }

    private void CheckForBarricades()
    {
        if (currentBarricadeTarget != null) return;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, attackRange);
        foreach (Collider2D col in colliders)
        {
            Barricade barricade = col.GetComponent<Barricade>();
            if (barricade != null && !barricade.IsDestroyed)
            {
                currentBarricadeTarget = col.transform;
                return;
            }
        }
    }

    private void AttackBarricade(Barricade barricade)
    {
        barricade.TakeDamage(damage);
        StartCoroutine(AttackVisualFeedback());
        Debug.Log($"Inimigo golpeou a Barricada! Dano: {damage} (Vida restante: {barricade.CurrentHealth}/{barricade.MaxHealth})");
    }

    private void AttackTower()
    {
        if (PlayerBase.Instance != null)
        {
            PlayerBase.Instance.TakeDamage(Mathf.RoundToInt(damage));
            StartCoroutine(AttackVisualFeedback());
            Debug.Log($"Inimigo está batendo na Base! Dano: {damage}");
        }
    }

    private IEnumerator AttackVisualFeedback()
    {
        if (spriteRenderer != null)
        {
            Color original = spriteRenderer.color;
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(0.08f);
            if (spriteRenderer != null) spriteRenderer.color = original;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
