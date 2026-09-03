using System.Collections;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class BarricadePlacement : MonoBehaviour
{
    public static BarricadePlacement Instance { get; private set; }

    [Header("Configurações de Construção")]
    [SerializeField] private GameObject barricadePrefab;
    [SerializeField] private int barricadeCost = 40;

    [Header("Restrições de Posicionamento")]
    [Tooltip("Largura máxima permitida da estrada para posicionar a barreira.")]
    [SerializeField] private float maxDistanceFromPath = 1.0f;
    [Tooltip("Distância mínima de outra barreira ou torre.")]
    [SerializeField] private float minClearanceDistance = 0.9f;

    [Header("Cores da Prévia")]
    [SerializeField] private Color validColor = new Color(0.3f, 1f, 0.3f, 0.6f); // Verde
    [SerializeField] private Color invalidColor = new Color(1f, 0.2f, 0.2f, 0.6f); // Vermelho

    private bool isPlacing = false;
    private bool canPlaceThisFrame = false;
    private GameObject previewObject;
    private SpriteRenderer[] previewRenderers;
    private Camera mainCamera;

    public int BarricadeCost => barricadeCost;
    public bool IsPlacing => isPlacing;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (!isPlacing) return;

        Vector3 mouseWorldPos = GetMouseWorldPosition();

        if (previewObject != null)
        {
            previewObject.transform.position = mouseWorldPos;
        }

        // Verifica se a posição atual está dentro do caminho e livre
        bool isValid = IsValidPlacementPosition(mouseWorldPos);
        UpdatePreviewColor(isValid);

        // Aguarda 1 frame após clicar no botão da UI para não colocar a barricada em cima do botão
        if (!canPlaceThisFrame) return;

        // Clique esquerdo no mapa para posicionar a barreira
        if (IsLeftMouseButtonPressed())
        {
            // Ignora se estiver com o mouse sobre qualquer elemento da UI
            if (UIHelper.IsPointerOverUI())
            {
                return;
            }

            if (isValid)
            {
                PlaceBarricade(mouseWorldPos);
            }
            else
            {
                Debug.LogWarning("A barreira só pode ser posicionada em cima do caminho e longe de outras barreiras!");
            }
        }

        // Clique direito ou ESC para cancelar
        if (IsCancelPressed())
        {
            CancelPlacement();
        }
    }

    public void StartPlacement()
    {
        if (CoinManager.Instance != null && !CoinManager.Instance.HasEnoughCoins(barricadeCost))
        {
            Debug.LogWarning("Moedas insuficientes para comprar uma barreira!");
            return;
        }

        isPlacing = true;
        canPlaceThisFrame = false;
        StartCoroutine(EnablePlacementNextFrame());

        if (barricadePrefab != null && previewObject == null)
        {
            previewObject = Instantiate(barricadePrefab);

            // Desativa colisores e scripts na prévia
            Collider2D[] cols = previewObject.GetComponentsInChildren<Collider2D>();
            foreach (Collider2D c in cols) c.enabled = false;

            Barricade barricadeScript = previewObject.GetComponent<Barricade>();
            if (barricadeScript != null) barricadeScript.enabled = false;

            previewRenderers = previewObject.GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer sr in previewRenderers)
            {
                sr.sortingOrder = 100;
            }
        }
    }

    private IEnumerator EnablePlacementNextFrame()
    {
        yield return null;
        canPlaceThisFrame = true;
    }

    private bool IsValidPlacementPosition(Vector3 position)
    {
        // 1. Deve estar dentro do caminho dos inimigos
        if (!IsPositionOnAnyPath(position))
        {
            return false;
        }

        // 2. Não pode estar em cima de outra barreira, torre ou spot de torre
        Collider2D[] overlapping = Physics2D.OverlapCircleAll(position, minClearanceDistance);
        foreach (Collider2D col in overlapping)
        {
            if (col.GetComponent<Barricade>() != null || 
                col.GetComponent<Tower>() != null || 
                col.GetComponent<TowerSpot>() != null ||
                col.GetComponent<PlayerBase>() != null)
            {
                return false;
            }
        }

        return true;
    }

    private bool IsPositionOnAnyPath(Vector3 position)
    {
        WaypointPath[] allPaths = FindObjectsOfType<WaypointPath>();
        if (allPaths == null || allPaths.Length == 0) return true; // Se não houver caminhos configurados, permite

        foreach (WaypointPath path in allPaths)
        {
            Transform[] waypoints = path.GetWaypoints();
            if (waypoints == null || waypoints.Length < 2) continue;

            for (int i = 0; i < waypoints.Length - 1; i++)
            {
                if (waypoints[i] == null || waypoints[i + 1] == null) continue;

                float distanceToSegment = DistancePointToSegment(position, waypoints[i].position, waypoints[i + 1].position);
                if (distanceToSegment <= maxDistanceFromPath)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private float DistancePointToSegment(Vector3 point, Vector3 segA, Vector3 segB)
    {
        Vector2 p = point;
        Vector2 a = segA;
        Vector2 b = segB;

        Vector2 ab = b - a;
        float abLengthSq = ab.sqrMagnitude;

        if (abLengthSq <= 0.0001f)
        {
            return Vector2.Distance(p, a);
        }

        float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / abLengthSq);
        Vector2 projection = a + t * ab;

        return Vector2.Distance(p, projection);
    }

    private void UpdatePreviewColor(bool isValid)
    {
        if (previewRenderers == null) return;

        Color targetColor = isValid ? validColor : invalidColor;
        foreach (SpriteRenderer sr in previewRenderers)
        {
            if (sr != null) sr.color = targetColor;
        }
    }

    private void PlaceBarricade(Vector3 position)
    {
        if (CoinManager.Instance != null && CoinManager.Instance.SpendCoins(barricadeCost))
        {
            Instantiate(barricadePrefab, position, Quaternion.identity);
            Debug.Log($"Barreira posicionada no caminho por {barricadeCost} moedas!");
            CancelPlacement();
        }
        else
        {
            Debug.LogWarning("Não foi possível pagar pela barreira.");
            CancelPlacement();
        }
    }

    public void CancelPlacement()
    {
        isPlacing = false;
        canPlaceThisFrame = false;
        if (previewObject != null)
        {
            Destroy(previewObject);
            previewObject = null;
        }
    }

    private bool IsLeftMouseButtonPressed()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            return Mouse.current.leftButton.wasPressedThisFrame;
        }
#endif
        return Input.GetMouseButtonDown(0);
    }

    private bool IsCancelPressed()
    {
#if ENABLE_INPUT_SYSTEM
        bool rightMouse = Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame;
        bool escKey = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
        return rightMouse || escKey;
#else
        return Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape);
#endif
    }

    private Vector3 GetMouseWorldPosition()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera == null) return Vector3.zero;

        Vector3 mouseScreenPos;

#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            Vector2 mPos = Mouse.current.position.ReadValue();
            mouseScreenPos = new Vector3(mPos.x, mPos.y, 0);
        }
        else
        {
            mouseScreenPos = Input.mousePosition;
        }
#else
        mouseScreenPos = Input.mousePosition;
#endif

        mouseScreenPos.z = -mainCamera.transform.position.z;
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos);
        worldPos.z = 0;
        return worldPos;
    }
}
