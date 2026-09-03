using System;
using System.Collections;
using UnityEngine;

public class Guard : MonoBehaviour
{
    public enum TroopType
    {
        Melee,   // Guerreiro: Bloqueia e ataca corpo a corpo
        Ranged   // Arqueiro: Fica posicionado e atira de longe
    }

    [Header("Tipo de Tropa")]
    [SerializeField] private TroopType troopType = TroopType.Melee;

    [Header("Atributos Básicos")]
    [SerializeField] private float maxHealth = 120f;
    [SerializeField] private float damage = 15f;
    [SerializeField] private float attackRate = 1f; // Ataques por segundo
    [SerializeField] private float moveSpeed = 2.5f;

    [Header("Alcances")]
    [Tooltip("Distância que o ataque alcança o inimigo")]
    [SerializeField] private float attackRange = 0.9f;
    [Tooltip("Raio para começar a mirar ou ir atrás do inimigo")]
    [SerializeField] private float detectionRange = 3.5f;

    [Header("Configurações Ranged (se for Arqueiro)")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;

    [Header("Detecção")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private string enemyTag = "Enemy";

    private float currentHealth;
    private float attackCountdown = 0f;
    private Vector3 guardPosition;
    private Transform currentTarget;
    private GuardBarracks parentBarracks;
    private SpriteRenderer spriteRenderer;

    public TroopType CurrentTroopType => troopType;
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public float Damage => damage;
    public bool IsDead => currentHealth <= 0;

    public event Action<float, float> OnHealthChanged;

    private void Awake()
    {
        if (firePoint == null) firePoint = transform;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        if (guardPosition == Vector3.zero) guardPosition = transform.position;
        InvokeRepeating(nameof(UpdateTarget), 0f, 0.15f);
    }

    public void Setup(GuardBarracks barracks, Vector3 assignedPosition)
    {
        parentBarracks = barracks;
        guardPosition = assignedPosition;
        transform.position = assignedPosition;
    }

    public void SetGuardPosition(Vector3 newPosition)
    {
        guardPosition = newPosition;
    }

    private void Update()
    {
        if (IsDead) return;

        if (currentTarget != null)
        {
            float distanceToEnemy = Vector2.Distance(transform.position, currentTarget.position);

            if (troopType == TroopType.Melee)
            {
                if (distanceToEnemy > attackRange)
                {
                    MoveTowards(currentTarget.position);
                }
                else
                {
                    FaceTarget(currentTarget.position);
                    if (attackCountdown <= 0f)
                    {
                        AttackMelee();
                        attackCountdown = 1f / attackRate;
                    }
                }
            }
            else if (troopType == TroopType.Ranged)
            {
                if (distanceToEnemy <= attackRange)
                {
                    FaceTarget(currentTarget.position);

                    if (attackCountdown <= 0f)
                    {
                        AttackRanged();
                        attackCountdown = 1f / attackRate;
                    }
                }
                else
                {
                    if (Vector2.Distance(transform.position, guardPosition) > 0.1f)
                    {
                        MoveTowards(guardPosition);
                    }
                }
            }
        }
        else
        {
            // Sem alvo: retorna para a posição do posto
            if (Vector2.Distance(transform.position, guardPosition) > 0.1f)
            {
                MoveTowards(guardPosition);
            }
        }

        if (attackCountdown > 0f)
        {
            attackCountdown -= Time.deltaTime;
        }
    }

    private void MoveTowards(Vector3 destination)
    {
        Vector3 direction = (destination - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;
        FaceTarget(destination);
    }

    private void FaceTarget(Vector3 destination)
    {
        Vector3 direction = (destination - transform.position).normalized;
        if (direction.x > 0.05f)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (direction.x < -0.05f)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }

    private void UpdateTarget()
    {
        // Se o alvo atual já morreu ou saiu muito de alcance
        if (currentTarget != null)
        {
            Enemy enemyComp = currentTarget.GetComponent<Enemy>();
            float maxTrack = (troopType == TroopType.Ranged ? attackRange : detectionRange) * 1.3f;
            if (enemyComp == null || enemyComp.IsDead || Vector2.Distance(transform.position, currentTarget.position) > maxTrack)
            {
                currentTarget = null;
            }
        }

        if (currentTarget == null)
        {
            float searchRange = troopType == TroopType.Ranged ? attackRange : detectionRange;
            float shortestDist = Mathf.Infinity;
            Transform bestTarget = null;

            // 1. Busca por Colisores usando Physics2D
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, searchRange);
            foreach (Collider2D col in colliders)
            {
                Enemy enemy = col.GetComponent<Enemy>();
                if ((enemy != null && !enemy.IsDead) || col.CompareTag(enemyTag))
                {
                    float dist = Vector2.Distance(transform.position, col.transform.position);
                    if (dist < shortestDist)
                    {
                        shortestDist = dist;
                        bestTarget = col.transform;
                    }
                }
            }

            // 2. Fallback robusto se nenhum collider respondeu
            if (bestTarget == null)
            {
                Enemy[] allEnemies = FindObjectsOfType<Enemy>();
                foreach (Enemy enemy in allEnemies)
                {
                    if (enemy != null && !enemy.IsDead)
                    {
                        float dist = Vector2.Distance(transform.position, enemy.transform.position);
                        if (dist <= searchRange && dist < shortestDist)
                        {
                            shortestDist = dist;
                            bestTarget = enemy.transform;
                        }
                    }
                }
            }

            currentTarget = bestTarget;
        }
    }

    private void AttackMelee()
    {
        if (currentTarget == null) return;

        Enemy enemy = currentTarget.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            StartCoroutine(AttackVisualFeedback());
            Debug.Log($"Guarda golpeou {enemy.gameObject.name} causando {damage} de dano!");
        }
    }

    private void AttackRanged()
    {
        if (currentTarget == null || projectilePrefab == null || firePoint == null) return;

        GameObject projObj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        Projectile proj = projObj.GetComponent<Projectile>();
        if (proj != null)
        {
            proj.Seek(currentTarget, damage);
        }
    }

    private IEnumerator AttackVisualFeedback()
    {
        if (spriteRenderer != null)
        {
            Color original = spriteRenderer.color;
            spriteRenderer.color = Color.yellow;
            yield return new WaitForSeconds(0.1f);
            if (spriteRenderer != null) spriteRenderer.color = original;
        }
    }

    public void TakeDamage(float incomingDamage)
    {
        if (IsDead) return;

        currentHealth -= incomingDamage;
        currentHealth = Mathf.Max(0, currentHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        StartCoroutine(DamageFlash());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator DamageFlash()
    {
        if (spriteRenderer != null)
        {
            Color orig = spriteRenderer.color;
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            if (spriteRenderer != null) spriteRenderer.color = orig;
        }
    }

    public void UpgradeStats(float healthBonus, float damageBonus)
    {
        maxHealth += healthBonus;
        damage += damageBonus;
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void Die()
    {
        if (parentBarracks != null)
        {
            parentBarracks.OnGuardDied(this);
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = troopType == TroopType.Melee ? Color.blue : Color.green;
        Vector3 origin = guardPosition != Vector3.zero ? guardPosition : transform.position;
        Gizmos.DrawWireSphere(origin, troopType == TroopType.Ranged ? attackRange : detectionRange);
    }
}
