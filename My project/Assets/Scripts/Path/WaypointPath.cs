using UnityEngine;

public class WaypointPath : MonoBehaviour
{
    public static WaypointPath Instance { get; private set; }

    [Header("Waypoints do Caminho")]
    [Tooltip("Se deixar vazio, os objetos filhos deste GameObject serão usados automaticamente como waypoints.")]
    [SerializeField] private Transform[] waypoints;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Popula automaticamente com os GameObjects filhos se o array estiver vazio
        if (waypoints == null || waypoints.Length == 0)
        {
            waypoints = new Transform[transform.childCount];
            for (int i = 0; i < transform.childCount; i++)
            {
                waypoints[i] = transform.GetChild(i);
            }
        }
    }

    public Transform[] GetWaypoints()
    {
        return waypoints;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        if (waypoints != null && waypoints.Length > 0)
        {
            for (int i = 0; i < waypoints.Length; i++)
            {
                if (waypoints[i] != null)
                {
                    Gizmos.DrawSphere(waypoints[i].position, 0.25f);
                    if (i < waypoints.Length - 1 && waypoints[i + 1] != null)
                    {
                        Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
                    }
                }
            }
        }
        else
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform current = transform.GetChild(i);
                Gizmos.DrawSphere(current.position, 0.25f);
                if (i < transform.childCount - 1)
                {
                    Transform next = transform.GetChild(i + 1);
                    Gizmos.DrawLine(current.position, next.position);
                }
            }
        }
    }
}
