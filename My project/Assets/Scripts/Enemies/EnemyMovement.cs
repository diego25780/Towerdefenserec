using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public enum MovementMode
    {
        DirectToTower,   // Corre em linha reta até a Torre/Base
        FollowWaypoints  // Segue os pontos do caminho
    }

    [Header("Tipo de Movimentação")]
    [SerializeField] private MovementMode movementMode = MovementMode.DirectToTower;
    [SerializeField] private float speed = 3.5f;
    [SerializeField] private bool rotateTowardsTarget = true;
    [SerializeField] private float rotationOffset = 0f;

    [Header("Alvo Principal")]
    [Tooltip("Se vazio, busca automaticamente a Torre ou Base na cena.")]
    [SerializeField] private Transform targetTower;
    [SerializeField] private int damageToTower = 5;

    [Header("Detecção de Obstáculos (Barreiras e Tropas)")]
    [SerializeField] private float obstacleCheckDistance = 0.5f;
    [SerializeField] private LayerMask obstacleLayer;

    private Transform[] waypoints;
    private int currentWaypointIndex = 0;
    private bool isBlockedByObstacle = false;

    public float Speed => speed;
    public bool IsBlocked => isBlockedByObstacle;

    private void Start()
    {
        // Encontra a Torre ou Base automaticamente se estiver no modo direto
        if (movementMode == MovementMode.DirectToTower && targetTower == null)
        {
            FindTargetTower();
        }
        else if (movementMode == MovementMode.FollowWaypoints && WaypointPath.Instance != null)
        {
            waypoints = WaypointPath.Instance.GetWaypoints();
        }
    }

    private void Update()
    {
        if (isBlockedByObstacle) return;

        if (movementMode == MovementMode.DirectToTower)
        {
            if (targetTower == null)
            {
                FindTargetTower();
                return;
            }

            MoveTowards(targetTower.position, OnReachTargetTower);
        }
        else if (movementMode == MovementMode.FollowWaypoints)
        {
            MoveAlongWaypoints();
        }
    }

    public void SetBlocked(bool blocked)
    {
        isBlockedByObstacle = blocked;
    }

    private void FindTargetTower()
    {
        // 1. Tenta achar objeto com script Tower
        Tower tower = FindObjectOfType<Tower>();
        if (tower != null)
        {
            targetTower = tower.transform;
            return;
        }

        // 2. Tenta achar PlayerBase
        if (PlayerBase.Instance != null)
        {
            targetTower = PlayerBase.Instance.transform;
            return;
        }

        // 3. Fallback por tags
        GameObject towerObj = GameObject.FindWithTag("Tower");
        if (towerObj != null)
        {
            targetTower = towerObj.transform;
        }
    }

    private void MoveAlongWaypoints()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        if (currentWaypointIndex >= waypoints.Length)
        {
            OnReachTargetTower();
            return;
        }

        Transform targetPoint = waypoints[currentWaypointIndex];
        if (targetPoint == null)
        {
            currentWaypointIndex++;
            return;
        }

        MoveTowards(targetPoint.position, () =>
        {
            currentWaypointIndex++;
            if (currentWaypointIndex >= waypoints.Length)
            {
                OnReachTargetTower();
            }
        });
    }

    private void MoveTowards(Vector3 destination, System.Action onReach)
    {
        Vector3 direction = (destination - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;

        if (rotateTowardsTarget && direction != Vector3.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle + rotationOffset);
        }

        if (Vector2.Distance(transform.position, destination) <= 0.2f)
        {
            onReach?.Invoke();
        }
    }

    private void OnReachTargetTower()
    {
        // Causa dano à base/torre
        if (PlayerBase.Instance != null)
        {
            PlayerBase.Instance.TakeDamage(damageToTower);
        }

        Destroy(gameObject);
    }
}
