using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public enum MovementMode
    {
        FollowWaypoints, // Segue o caminho traçado pelos waypoints (Padrão)
        DirectToTower    // Corre em linha reta (se não houver caminho)
    }

    [Header("Tipo de Movimentação")]
    [SerializeField] private MovementMode movementMode = MovementMode.FollowWaypoints;
    [SerializeField] private float speed = 3.5f;
    [SerializeField] private bool rotateTowardsTarget = true;
    [SerializeField] private float rotationOffset = 0f;

    [Header("Alvo Principal (Base / Torre)")]
    [SerializeField] private Transform targetTower;
    [SerializeField] private float stopDistanceToTower = 0.8f;

    private Transform[] waypoints;
    private int currentWaypointIndex = 0;
    private bool isBlockedByObstacle = false;
    private Transform blockingObstacle = null;
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

        // Se waypoints não foi atribuído pelo Spawner, busca um caminho na cena
        if (movementMode == MovementMode.FollowWaypoints && (waypoints == null || waypoints.Length == 0))
        {
            WaypointPath path = FindObjectOfType<WaypointPath>();
            if (path != null)
            {
                waypoints = path.GetWaypoints();
            }
        }
    }

    public void SetPath(Transform[] newPath)
    {
        waypoints = newPath;
        currentWaypointIndex = 0;
        movementMode = MovementMode.FollowWaypoints;

        if (waypoints != null && waypoints.Length > 0 && waypoints[0] != null)
        {
            transform.position = waypoints[0].position;
        }
    }

    private void Update()
    {
        // Se o obstáculo que estava bloqueando foi destruído, desbloqueia o movimento imediatamente
        if (isBlockedByObstacle)
        {
            if (blockingObstacle == null || !blockingObstacle.gameObject.activeInHierarchy)
            {
                isBlockedByObstacle = false;
                blockingObstacle = null;
            }
            else
            {
                return;
            }
        }

        if (hasReachedTower) return;

        if (movementMode == MovementMode.FollowWaypoints && waypoints != null && waypoints.Length > 0)
        {
            MoveAlongWaypoints();
        }
        else
        {
            // Fallback direto para a torre se não houver waypoints
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
    }

    public void SetBlocked(bool blocked, Transform obstacle = null)
    {
        isBlockedByObstacle = blocked;
        blockingObstacle = obstacle;
    }

    private void FindTargetTower()
    {
        if (PlayerBase.Instance != null)
        {
            targetTower = PlayerBase.Instance.transform;
            return;
        }

        Tower tower = FindObjectOfType<Tower>();
        if (tower != null)
        {
            targetTower = tower.transform;
            return;
        }

        GameObject baseObj = GameObject.FindWithTag("PlayerBase");
        if (baseObj == null) baseObj = GameObject.FindWithTag("Tower");
        if (baseObj != null) targetTower = baseObj.transform;
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
        transform.position = Vector3.MoveTowards(transform.position, destination, speed * Time.deltaTime);

        Vector3 direction = (destination - transform.position).normalized;
        if (rotateTowardsTarget && direction != Vector3.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle + rotationOffset);
        }

        if (Vector2.Distance(transform.position, destination) <= 0.25f)
        {
            onReach?.Invoke();
        }
    }
}
