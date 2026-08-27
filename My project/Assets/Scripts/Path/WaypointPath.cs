using System.Collections.Generic;
using UnityEngine;

public class WaypointPath : MonoBehaviour
{
    public static List<WaypointPath> AllPaths { get; private set; } = new List<WaypointPath>();

    [Header("Configuração do Caminho")]
    [SerializeField] private string pathName = "Caminho Principal";
    [SerializeField] private Color gizmoColor = Color.green;

    [Header("Waypoints")]
    [Tooltip("Se deixar vazio, os objetos filhos deste GameObject serão usados automaticamente como waypoints.")]
    [SerializeField] private Transform[] waypoints;

    public string PathName => pathName;

    private void Awake()
    {
        if (!AllPaths.Contains(this))
        {
            AllPaths.Add(this);
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

    private void OnDestroy()
    {
        AllPaths.Remove(this);
    }

    public Transform[] GetWaypoints()
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            waypoints = new Transform[transform.childCount];
            for (int i = 0; i < transform.childCount; i++)
            {
                waypoints[i] = transform.GetChild(i);
            }
        }
        return waypoints;
    }

    public Vector3 GetStartPoint()
    {
        Transform[] pts = GetWaypoints();
        if (pts != null && pts.Length > 0 && pts[0] != null)
        {
            return pts[0].position;
        }
        return transform.position;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;

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
