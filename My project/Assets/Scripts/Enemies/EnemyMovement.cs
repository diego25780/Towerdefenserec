using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("Movimentação")]
    [SerializeField] private float speed = 3f;
    [SerializeField] private bool rotateTowardsTarget = true;
    [SerializeField] private float rotationOffset = 0f; // Ajuste de rotação para alinhar com o sprite

    [Header("Destino")]
    [Tooltip("Deixe vazio se estiver usando o sistema de Waypoints.")]
    [SerializeField] private Transform directTarget;
    [SerializeField] private int damageToBase = 1;

    private Transform[] waypoints;
    private int currentWaypointIndex = 0;

    private void Start()
    {
        // Se houver um WaypointPath na cena e nenhum alvo direto foi definido
        if (directTarget == null && WaypointPath.Instance != null)
        {
            waypoints = WaypointPath.Instance.GetWaypoints();
        }
    }

    private void Update()
    {
        if (waypoints != null && waypoints.Length > 0)
        {
            MoveAlongWaypoints();
        }
        else if (directTarget != null)
        {
            MoveTowards(directTarget.position, OnReachDestination);
        }
    }

    public void SetPath(Transform[] customWaypoints)
    {
        waypoints = customWaypoints;
        currentWaypointIndex = 0;
    }

    public void SetDirectTarget(Transform target)
    {
        directTarget = target;
    }

    private void MoveAlongWaypoints()
    {
        if (currentWaypointIndex >= waypoints.Length)
        {
            OnReachDestination();
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
                OnReachDestination();
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

        if (Vector2.Distance(transform.position, destination) <= 0.1f)
        {
            onReach?.Invoke();
        }
    }

    private void OnReachDestination()
    {
        // Aplica dano à base do jogador se o PlayerBase existir na cena
        if (PlayerBase.Instance != null)
        {
            PlayerBase.Instance.TakeDamage(damageToBase);
        }

        Destroy(gameObject);
    }
}
