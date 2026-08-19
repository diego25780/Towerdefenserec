using UnityEngine;

public class Tower : MonoBehaviour
{
    public enum TargetPriority
    {
        Nearest,
        LowestHealth
    }

    [Header("Atributos da Torre")]
    [SerializeField] private float range = 5f;
    [SerializeField] private float fireRate = 1f; // Tiros por segundo
    [SerializeField] private float damage = 25f;
    [SerializeField] private TargetPriority priority = TargetPriority.Nearest;

    [Header("Configurações de Tiro")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private Transform partToRotate; // Cabeça / canhão da torre para girar (opcional)
    [SerializeField] private float rotationOffset = -90f; // Geralmente -90 se o sprite aponta para cima

    [Header("Detecção")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private string enemyTag = "Enemy";

    private Transform currentTarget;
    private float fireCountdown = 0f;

    public float Range => range;
    public float Damage => damage;
    public float FireRate => fireRate;

    private void Start()
    {
        if (firePoint == null)
        {
            firePoint = transform;
        }

        // Atualiza a busca de alvos periodicamente para melhor desempenho
        InvokeRepeating(nameof(UpdateTarget), 0f, 0.15f);
    }

    private void Update()
    {
        if (currentTarget == null) return;

        // Se o alvo saiu do alcance ou foi destruído
        if (Vector2.Distance(transform.position, currentTarget.position) > range)
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
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, range, enemyLayer);
        
        float shortestDistance = Mathf.Infinity;
        float lowestHealth = Mathf.Infinity;
        Transform chosenEnemy = null;

        foreach (Collider2D col in colliders)
        {
            Enemy enemy = col.GetComponent<Enemy>();
            if (enemy == null && !col.CompareTag(enemyTag)) continue;

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

        // Fallback: se nenhum layer estiver selecionado e nada foi encontrado pelo OverlapCircleAll
        if (chosenEnemy == null && enemyLayer.value == 0)
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
            foreach (GameObject enemyObj in enemies)
            {
                float distance = Vector2.Distance(transform.position, enemyObj.transform.position);
                if (distance <= range && distance < shortestDistance)
                {
                    shortestDistance = distance;
                    chosenEnemy = enemyObj.transform;
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
            projectile.Seek(currentTarget, damage);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
