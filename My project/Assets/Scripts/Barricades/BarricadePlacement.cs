using UnityEngine;
using UnityEngine.EventSystems;

public class BarricadePlacement : MonoBehaviour
{
    public static BarricadePlacement Instance { get; private set; }

    [Header("Configurações de Construção")]
    [SerializeField] private GameObject barricadePrefab;
    [SerializeField] private int barricadeCost = 40;
    [SerializeField] private LayerMask blockedLayers; // Ex: não colocar em cima de outra barreira/torre

    private bool isPlacing = false;
    private GameObject previewObject;
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

        // Clique esquerdo para posicionar
        if (Input.GetMouseButtonDown(0))
        {
            // Evita clicar através de elementos da UI
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            PlaceBarricade(mouseWorldPos);
        }

        // Clique direito ou ESC para cancelar
        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
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

        if (barricadePrefab != null && previewObject == null)
        {
            previewObject = Instantiate(barricadePrefab);
            
            // Desativa colliders e scripts no objeto de preview
            Collider2D col = previewObject.GetComponent<Collider2D>();
            if (col != null) col.enabled = false;

            Barricade barricadeScript = previewObject.GetComponent<Barricade>();
            if (barricadeScript != null) barricadeScript.enabled = false;

            // Deixa semi-transparente
            SpriteRenderer sr = previewObject.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Color c = sr.color;
                c.a = 0.5f;
                sr.color = c;
            }
        }
    }

    private void PlaceBarricade(Vector3 position)
    {
        if (CoinManager.Instance != null && CoinManager.Instance.SpendCoins(barricadeCost))
        {
            Instantiate(barricadePrefab, position, Quaternion.identity);
            Debug.Log($"Barreira construída por {barricadeCost} moedas!");
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
        if (previewObject != null)
        {
            Destroy(previewObject);
            previewObject = null;
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = -mainCamera.transform.position.z;
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos);
        worldPos.z = 0;
        return worldPos;
    }
}
