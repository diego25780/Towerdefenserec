using System;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class Tower : MonoBehaviour
{
    public enum TargetPriority
    {
        Nearest,
        LowestHealth
    }

    public static Tower SelectedTower { get; private set; }

    [Header("Identificação")]
    [SerializeField] private string towerName = "Canhão Padrão";

    [Header("Atributos da Torre")]
    [SerializeField] private float baseRange = 4.5f;
    [SerializeField] private float fireRate = 1.2f; // Tiros por segundo
    [SerializeField] private float baseDamage = 20f;
    [SerializeField] private TargetPriority priority = TargetPriority.Nearest;

    [Header("Evolução de Dano (Limite de Níveis)")]
    [SerializeField] private int baseDamageCost = 30;
    [SerializeField] private float damageCostMultiplier = 1.4f;
    [SerializeField] private float damageBonusPerLevel = 10f;
    [SerializeField] private int maxDamageLevel = 4;

    [Header("Evolução de Alcance (Limite de Níveis)")]
    [SerializeField] private int baseRangeCost = 25;
    [SerializeField] private float rangeCostMultiplier = 1.4f;
    [SerializeField] private float rangeBonusPerLevel = 0.8f;
    [SerializeField] private int maxRangeLevel = 4;

    [Header("Economia e Venda")]
    [SerializeField] private int initialBuildCost = 70;
    [SerializeField] private float sellRefundPercent = 0.7f; // Devolve 70% do valor total investido

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
    private int totalInvested;

    private TowerSpot parentSpot;
    private Transform currentTarget;
    private float fireCountdown = 0f;
    private Camera mainCam;

    public string TowerName => towerName;
    public float Range => currentRange;
    public float Damage => currentDamage;
    public float FireRate => fireRate;
    public int DamageLevel => damageLevel;
    public int RangeLevel => rangeLevel;
    public int MaxDamageLevel => maxDamageLevel;
    public int MaxRangeLevel => maxRangeLevel;

    public bool CanUpgradeDamage => damageLevel < maxDamageLevel;
    public bool CanUpgradeRange => rangeLevel < maxRangeLevel;

    public int CurrentDamageUpgradeCost => Mathf.RoundToInt(baseDamageCost * Mathf.Pow(damageCostMultiplier, damageLevel - 1));
    public int CurrentRangeUpgradeCost => Mathf.RoundToInt(baseRangeCost * Mathf.Pow(rangeCostMultiplier, rangeLevel - 1));
    public int SellValue => Mathf.Max(10, Mathf.RoundToInt(totalInvested * sellRefundPercent));

    public static event Action<Tower> OnTowerSelected;
    public static event Action OnTowerDeselected;
    public event Action OnTowerUpgraded;

    private void Awake()
    {
        currentDamage = baseDamage;
        currentRange = baseRange;
        totalInvested = initialBuildCost;
        SelectedTower = this;
        mainCam = Camera.main;

        // Garante que o sprite da torre fique visível na frente do mapa e do spot
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null && sr.sortingOrder < 10)
        {
            sr.sortingOrder = 10;
        }

        // Garante que a torre tenha colisor 2D para clique
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            CircleCollider2D circle = gameObject.AddComponent<CircleCollider2D>();
            circle.radius = 0.6f;
            circle.isTrigger = true;
        }
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

    public void SetParentSpot(TowerSpot spot, int costPaid)
    {
        parentSpot = spot;
        totalInvested = costPaid;
    }

    public void SelectTower()
    {
        SelectedTower = this;
        OnTowerSelected?.Invoke(this);
        Debug.Log($"Torre selecionada: {towerName} (Dano Nv {damageLevel}/{maxDamageLevel}, Alcance Nv {rangeLevel}/{maxRangeLevel})");
    }

    private void Update()
    {
        // Detecção de clique no New Input System
        if (IsLeftMouseClicked())
        {
            if (!UIHelper.IsPointerOverUI())
            {
                Vector3 mousePos = GetMouseWorldPos();
                Collider2D col = GetComponent<Collider2D>();
                if (col != null && col.OverlapPoint(mousePos))
                {
                    SelectTower();
                }
            }
        }

        if (currentTarget == null) return;

        if (Vector2.Distance(transform.position, currentTarget.position) > currentRange)
        {
            currentTarget = null;
            return;
        }

        if (partToRotate != null)
        {
            RotateTowardsTarget();
        }

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

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, currentRange);
        foreach (Collider2D col in colliders)
        {
            Enemy enemy = col.GetComponent<Enemy>();
            if (enemy == null && !col.CompareTag(enemyTag)) continue;
            if (enemy != null && enemy.IsDead) continue;

            float distanceToEnemy = Vector2.Distance(transform.position, col.transform.position);

            if (priority == TargetPriority.Nearest)
            {
                if (distanceToEnemy < shortestDistance)
                {
                    shortestDistance = distanceToEnemy;
                    chosenEnemy = col.transform;
                }
            }
            else if (priority == TargetPriority.LowestHealth && enemy != null)
            {
                if (enemy.CurrentHealth < lowestHealth)
                {
                    lowestHealth = enemy.CurrentHealth;
                    chosenEnemy = col.transform;
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
        if (projectilePrefab == null) return;

        GameObject projGO = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        Projectile projectile = projGO.GetComponent<Projectile>();

        if (projectile != null)
        {
            projectile.Seek(currentTarget, currentDamage);
        }
    }

    public bool UpgradeDamage()
    {
        if (!CanUpgradeDamage)
        {
            Debug.LogWarning("Dano da torre já atingiu o nível máximo!");
            return false;
        }

        int cost = CurrentDamageUpgradeCost;
        if (CoinManager.Instance != null && CoinManager.Instance.SpendCoins(cost))
        {
            damageLevel++;
            currentDamage += damageBonusPerLevel;
            totalInvested += cost;
            OnTowerUpgraded?.Invoke();
            Debug.Log($"Dano da torre evoluído para Nv {damageLevel}! Dano atual: {currentDamage}");
            return true;
        }

        Debug.LogWarning("Moedas insuficientes para evoluir o dano.");
        return false;
    }

    public bool UpgradeRange()
    {
        if (!CanUpgradeRange)
        {
            Debug.LogWarning("Alcance da torre já atingiu o nível máximo!");
            return false;
        }

        int cost = CurrentRangeUpgradeCost;
        if (CoinManager.Instance != null && CoinManager.Instance.SpendCoins(cost))
        {
            rangeLevel++;
            currentRange += rangeBonusPerLevel;
            totalInvested += cost;
            OnTowerUpgraded?.Invoke();
            Debug.Log($"Alcance da torre evoluído para Nv {rangeLevel}! Alcance atual: {currentRange}");
            return true;
        }

        Debug.LogWarning("Moedas insuficientes para evoluir o alcance.");
        return false;
    }

    public void SellTower()
    {
        int refund = SellValue;
        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.AddCoins(refund);
        }

        if (parentSpot != null)
        {
            parentSpot.OnTowerSold();
        }

        if (SelectedTower == this)
        {
            SelectedTower = null;
            OnTowerDeselected?.Invoke();
        }

        Debug.Log($"Torre '{towerName}' vendida por {refund}$!");
        Destroy(gameObject);
    }

    private bool IsLeftMouseClicked()
    {
#if ENABLE_INPUT_SYSTEM
        return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
#else
        return Input.GetMouseButtonDown(0);
#endif
    }

    private Vector3 GetMouseWorldPos()
    {
        if (mainCam == null) mainCam = Camera.main;
        if (mainCam == null) return Vector3.zero;

        Vector3 screenPos;
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            Vector2 m = Mouse.current.position.ReadValue();
            screenPos = new Vector3(m.x, m.y, 0);
        }
        else screenPos = Input.mousePosition;
#else
        screenPos = Input.mousePosition;
#endif
        screenPos.z = -mainCam.transform.position.z;
        Vector3 world = mainCam.ScreenToWorldPoint(screenPos);
        world.z = 0;
        return world;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, currentRange > 0 ? currentRange : baseRange);
    }
}
