using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [Header("Ataque do Inimigo")]
    [SerializeField] private float damage = 12f;
    [SerializeField] private float attackRange = 0.8f;
    [SerializeField] private float attackRate = 1f; // Golpes por segundo

    [Header("Detecção de Obstáculos (Barreiras / Guardas)")]
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private string guardTag = "Guard";
    [SerializeField] private string barricadeTag = "Barricade";

    private float attackCountdown = 0f;
    private EnemyMovement enemyMovement;
    private Transform currentObstacleTarget;

    public bool IsAttacking => currentObstacleTarget != null;

    private void Awake()
    {
        enemyMovement = GetComponent<EnemyMovement>();
    }

    private void Start()
    {
        InvokeRepeating(nameof(CheckForObstacles), 0f, 0.15f);
    }

    private void Update()
    {
        if (currentObstacleTarget == null)
        {
            if (enemyMovement != null && enemyMovement.IsBlocked)
            {
                enemyMovement.SetBlocked(false);
            }
            return;
        }

        // Se o obstáculo foi destruído ou o alvo sumiu
        if (IsTargetInvalid(currentObstacleTarget))
        {
            currentObstacleTarget = null;
            if (enemyMovement != null) enemyMovement.SetBlocked(false);
            return;
        }

        float distance = Vector2.Distance(transform.position, currentObstacleTarget.position);

        if (distance <= attackRange)
        {
            if (enemyMovement != null)
            {
                enemyMovement.SetBlocked(true);
            }

            if (attackCountdown <= 0f)
            {
                PerformAttack();
                attackCountdown = 1f / attackRate;
            }
        }
        else
        {
            if (enemyMovement != null)
            {
                enemyMovement.SetBlocked(false);
            }
        }

        if (attackCountdown > 0f)
        {
            attackCountdown -= Time.deltaTime;
        }
    }

    private void CheckForObstacles()
    {
        if (currentObstacleTarget != null && !IsTargetInvalid(currentObstacleTarget))
        {
            return;
        }

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, attackRange, obstacleLayer);
        float shortestDist = Mathf.Infinity;
        Transform closestObstacle = null;

        foreach (Collider2D col in colliders)
        {
            // Verifica se é uma Barreira ou um Guarda
            Barricade barricade = col.GetComponent<Barricade>();
            if (barricade != null && !barricade.IsDestroyed)
            {
                float dist = Vector2.Distance(transform.position, col.transform.position);
                if (dist < shortestDist)
                {
                    shortestDist = dist;
                    closestObstacle = col.transform;
                }
                continue;
            }

            Guard guard = col.GetComponent<Guard>();
            if (guard != null && !guard.IsDead)
            {
                float dist = Vector2.Distance(transform.position, col.transform.position);
                if (dist < shortestDist)
                {
                    shortestDist = dist;
                    closestObstacle = col.transform;
                }
                continue;
            }

            if (col.CompareTag(barricadeTag) || col.CompareTag(guardTag))
            {
                float dist = Vector2.Distance(transform.position, col.transform.position);
                if (dist < shortestDist)
                {
                    shortestDist = dist;
                    closestObstacle = col.transform;
                }
            }
        }

        // Fallback por tags se obstacleLayer for 0
        if (closestObstacle == null && obstacleLayer.value == 0)
        {
            CheckTagFallback(barricadeTag, ref closestObstacle, ref shortestDist);
            CheckTagFallback(guardTag, ref closestObstacle, ref shortestDist);
        }

        currentObstacleTarget = closestObstacle;
    }

    private void CheckTagFallback(string tag, ref Transform closest, ref float shortestDist)
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag(tag);
        foreach (GameObject obj in objs)
        {
            float dist = Vector2.Distance(transform.position, obj.transform.position);
            if (dist <= attackRange && dist < shortestDist)
            {
                shortestDist = dist;
                closest = obj.transform;
            }
        }
    }

    private bool IsTargetInvalid(Transform target)
    {
        if (target == null) return true;

        Barricade barricade = target.GetComponent<Barricade>();
        if (barricade != null && barricade.IsDestroyed) return true;

        Guard guard = target.GetComponent<Guard>();
        if (guard != null && guard.IsDead) return true;

        return false;
    }

    private void PerformAttack()
    {
        if (currentObstacleTarget == null) return;

        Barricade barricade = currentObstacleTarget.GetComponent<Barricade>();
        if (barricade != null)
        {
            barricade.TakeDamage(damage);
            return;
        }

        Guard guard = currentObstacleTarget.GetComponent<Guard>();
        if (guard != null)
        {
            guard.TakeDamage(damage);
            return;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
