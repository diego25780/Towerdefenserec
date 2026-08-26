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

    [Header("Alvo Principal (Torre / Base)")]
    [Tooltip("Se vazio, busca automaticamente a Torre ou Base na cena.")]
    [SerializeField] private Transform targetTower;
    [SerializeField] private float stopDistanceToTower = 0.8f;

    private Transform[] waypoints;
    private int currentWaypointIndex = 0;
    private bool isBlockedByObstacle = false;
    private bool hasReachedTower = false;

    public float Speed => speed;
    public bool IsBlocked => isBlockedByObstacle;
    public bool HasReachedTower => hasReachedTower;
    public Transform TargetTower => targetTower;

    private void Start()
    {
        if (targetTower == null)
        {
            FindTargetTower();
        }

        if (movementMode == MovementMode.FollowWaypoints && WaypointPath.Instance != null)
        {
            waypoints = WaypointPath.Instance.GetWaypoints();
        }
    }

    private void Update()
    {
        // Se estiver bloqueado por uma barricada ou já chegou na torre para bater
        if (isBlockedByObstacle || hasReachedTower) return;

        if (movementMode == MovementMode.DirectToTower)
        {
            if (targetTower == null)
            {
                FindTargetTower();
                return;
            }

            float distToTower = Vector2.Distance(transform.position, targetTower.position);
            if (distToTower <= stopDistanceToTower)
            {
                hasReachedTower = true;
                return;
            }

            MoveTowards(targetTower.position, () => { hasReachedTower = true; });
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
        Tower tower = FindObjectOfType<Tower>();
        if (tower != null)
        {
            targetTower = tower.transform;
            return;
        }

        if (PlayerBase.Instance != null)
        {
            targetTower = PlayerBase.Instance.transform;
            return;
        }

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
            hasReachedTower = true;
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
                hasReachedTower = true;
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

        if (Vector2.Distance(transform.position, destination) <= 0.15f)
        {
            onReach?.Invoke();
        }
    }
}
