using System;
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
    [SerializeField] private float attackRange = 0.8f; // Maior se for Ranged (ex: 4.5f)
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

    public TroopType CurrentTroopType => troopType;
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public float Damage => damage;
    public bool IsDead => currentHealth <= 0;

    public event Action<float, float> OnHealthChanged;

    private void Awake()
    {
        if (firePoint == null) firePoint = transform;
    }

    private void Start()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        InvokeRepeating(nameof(UpdateTarget), 0f, 0.2f);
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
                    if (attackCountdown <= 0f)
                    {
                        AttackMelee();
                        attackCountdown = 1f / attackRate;
                    }
                }
            }
            else if (troopType == TroopType.Ranged)
            {
                // Arqueiro: permanece no posto de guarda e atira à distância
                if (distanceToEnemy <= attackRange)
                {
                    // Vira para o alvo
                    FaceTarget(currentTarget.position);

                    if (attackCountdown <= 0f)
                    {
                        AttackRanged();
                        attackCountdown = 1f / attackRate;
                    }
                }
                else
                {
                    // Se o inimigo saiu do alcance de tiro, volta para o posto
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
        if (currentTarget != null)
        {
            Enemy enemy = currentTarget.GetComponent<Enemy>();
            float maxTrackRange = troopType == TroopType.Ranged ? attackRange : detectionRange;
            if (enemy == null || enemy.IsDead || Vector2.Distance(guardPosition, currentTarget.position) > maxTrackRange)
            {
                currentTarget = null;
            }
        }

        if (currentTarget == null)
        {
            float searchRange = troopType == TroopType.Ranged ? attackRange : detectionRange;
            Collider2D[] colliders = Physics2D.OverlapCircleAll(guardPosition, searchRange, enemyLayer);
            float shortestDist = Mathf.Infinity;
            Transform closestEnemy = null;

            foreach (Collider2D col in colliders)
            {
                Enemy enemy = col.GetComponent<Enemy>();
                if ((enemy != null && !enemy.IsDead) || col.CompareTag(enemyTag))
                {
                    float dist = Vector2.Distance(guardPosition, col.transform.position);
                    if (dist < shortestDist)
                    {
                        shortestDist = dist;
                        closestEnemy = col.transform;
                    }
                }
            }

            if (closestEnemy == null && enemyLayer.value == 0)
            {
                GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
                foreach (GameObject enemyObj in enemies)
                {
                    float dist = Vector2.Distance(guardPosition, enemyObj.transform.position);
                    if (dist <= searchRange && dist < shortestDist)
                    {
                        shortestDist = dist;
                        closestEnemy = enemyObj.transform;
                    }
                }
            }

            currentTarget = closestEnemy;
        }
    }

    private void AttackMelee()
    {
        if (currentTarget == null) return;

        Enemy enemy = currentTarget.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
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

    public void TakeDamage(float incomingDamage)
    {
        if (IsDead) return;

        currentHealth -= incomingDamage;
        currentHealth = Mathf.Max(0, currentHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
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
