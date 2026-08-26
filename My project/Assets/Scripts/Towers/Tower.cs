using System;
using UnityEngine;

public class Tower : MonoBehaviour
{
    public enum TargetPriority
    {
        Nearest,
        LowestHealth
    }

    public static Tower SelectedTower { get; private set; }

    [Header("Identificação")]
    [SerializeField] private string towerName = "Torre de Flechas";

    [Header("Atributos da Torre")]
    [SerializeField] private float baseRange = 4.5f;
    [SerializeField] private float fireRate = 1.2f; // Tiros por segundo
    [SerializeField] private float baseDamage = 20f;
    [SerializeField] private TargetPriority priority = TargetPriority.Nearest;

    [Header("Evolução de Dano")]
    [SerializeField] private int baseDamageCost = 30;
    [SerializeField] private float damageCostMultiplier = 1.4f;
    [SerializeField] private float damageBonusPerLevel = 10f;

    [Header("Evolução de Alcance")]
    [SerializeField] private int baseRangeCost = 25;
    [SerializeField] private float rangeCostMultiplier = 1.4f;
    [SerializeField] private float rangeBonusPerLevel = 0.8f;

    [Header("Configurações de Tiro")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private Transform partToRotate; // Cabeça / canhão da torre para girar (opcional)
    [SerializeField] private float rotationOffset = -90f; // Ajuste de rotação para o sprite

    [Header("Detecção")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private string enemyTag = "Enemy";

    private int damageLevel = 1;
    private int rangeLevel = 1;
    private float currentDamage;
    private float currentRange;

    private Transform currentTarget;
    private float fireCountdown = 0f;

    public string TowerName => towerName;
    public float Range => currentRange;
    public float Damage => currentDamage;
    public float FireRate => fireRate;
    public int DamageLevel => damageLevel;
    public int RangeLevel => rangeLevel;

    public int CurrentDamageUpgradeCost => Mathf.RoundToInt(baseDamageCost * Mathf.Pow(damageCostMultiplier, damageLevel - 1));
    public int CurrentRangeUpgradeCost => Mathf.RoundToInt(baseRangeCost * Mathf.Pow(rangeCostMultiplier, rangeLevel - 1));

    public static event Action<Tower> OnTowerSelected;
    public event Action OnTowerUpgraded;

    private void Awake()
    {
        currentDamage = baseDamage;
        currentRange = baseRange;
        SelectedTower = this;
    }

    private void Start()
    {
        if (firePoint == null)
        {
            firePoint = transform;
        }

        InvokeRepeating(nameof(UpdateTarget), 0f, 0.12f);
        OnTowerSelected?.Invoke(this);
    }

    private void OnMouseDown()
    {
        SelectTower();
    }

    public void SelectTower()
    {
        SelectedTower = this;
        OnTowerSelected?.Invoke(this);
        Debug.Log($"Torre selecionada: {towerName} (Dano Nv {damageLevel}, Alcance Nv {rangeLevel})");
    }

    private void Update()
    {
        if (currentTarget == null) return;

        // Se o alvo saiu do alcance ou foi destruído
        if (Vector2.Distance(transform.position, currentTarget.position) > currentRange)
        {
            currentTarget = null;
            return;
        }

        // Rotaciona em direção ao alvo
        if (partToRotate != null)
        {
            RotateTowardsTarget();
        }

        // Contador de disparo
        if (fireCountdown <= 0f)
        {
            Shoot();
            fireCountdown = 1f / fireRate;
        }

        fireCountdown -= Time.deltaTime;
    }

    private void UpdateTarget()
    {
        float shortestDistance = Mathf.Infinity;
        float lowestHealth = Mathf.Infinity;
        Transform chosenEnemy = null;

        // 1. Busca usando Physics2D
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, currentRange);
        foreach (Collider2D col in colliders)
        {
            Enemy enemy = col.GetComponent<Enemy>();
            if (enemy == null && !col.CompareTag(enemyTag)) continue;
            if (enemy != null && enemy.IsDead) continue;

            float distance = Vector2.Distance(transform.position, col.transform.position);

            if (priority == TargetPriority.Nearest)
            {
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    chosenEnemy = col.transform;
                }
            }
            else if (priority == TargetPriority.LowestHealth)
            {
                float health = enemy != null ? enemy.CurrentHealth : 100f;
                if (health < lowestHealth)
                {
                    lowestHealth = health;
                    chosenEnemy = col.transform;
                }
            }
        }

        // 2. Fallback por componentes se nenhum collider foi achado
        if (chosenEnemy == null)
        {
            Enemy[] allEnemies = FindObjectsOfType<Enemy>();
            foreach (Enemy enemy in allEnemies)
            {
                if (enemy != null && !enemy.IsDead)
                {
                    float distance = Vector2.Distance(transform.position, enemy.transform.position);
                    if (distance <= currentRange && distance < shortestDistance)
                    {
                        shortestDistance = distance;
                        chosenEnemy = enemy.transform;
                    }
                }
            }
        }

        currentTarget = chosenEnemy;
    }

    private void RotateTowardsTarget()
    {
        Vector3 dir = currentTarget.position - partToRotate.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        partToRotate.rotation = Quaternion.Euler(0, 0, angle + rotationOffset);
    }

    private void Shoot()
    {
        if (projectilePrefab == null || firePoint == null) return;

        GameObject projectileObj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        Projectile projectile = projectileObj.GetComponent<Projectile>();

        if (projectile != null)
        {
            projectile.Seek(currentTarget, currentDamage);
        }
    }

    public bool UpgradeDamage()
    {
        int cost = CurrentDamageUpgradeCost;

        if (CoinManager.Instance != null && CoinManager.Instance.SpendCoins(cost))
        {
            damageLevel++;
            currentDamage = baseDamage + (damageLevel - 1) * damageBonusPerLevel;
            OnTowerUpgraded?.Invoke();
            Debug.Log($"{towerName} evoluiu para Dano Nível {damageLevel}! (Dano atual: {currentDamage})");
            return true;
        }

        Debug.LogWarning("Moedas insuficientes para evoluir o dano da torre!");
        return false;
    }

    public bool UpgradeRange()
    {
        int cost = CurrentRangeUpgradeCost;

        if (CoinManager.Instance != null && CoinManager.Instance.SpendCoins(cost))
        {
            rangeLevel++;
            currentRange = baseRange + (rangeLevel - 1) * rangeBonusPerLevel;
            OnTowerUpgraded?.Invoke();
            Debug.Log($"{towerName} evoluiu para Alcance Nível {rangeLevel}! (Alcance atual: {currentRange})");
            return true;
        }

        Debug.LogWarning("Moedas insuficientes para evoluir o alcance da torre!");
        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        float displayRange = currentRange > 0 ? currentRange : baseRange;
        Gizmos.DrawWireSphere(transform.position, displayRange);
    }
}
