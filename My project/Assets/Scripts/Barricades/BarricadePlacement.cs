using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class BarricadePlacement : MonoBehaviour
{
    public static BarricadePlacement Instance { get; private set; }

    [Header("Configurações de Construção")]
    [SerializeField] private GameObject barricadePrefab;
    [SerializeField] private int barricadeCost = 40;
    [SerializeField] private LayerMask blockedLayers;

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
        if (IsLeftMouseButtonPressed())
        {
            // Evita clicar através de elementos da UI
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            PlaceBarricade(mouseWorldPos);
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

        if (barricadePrefab != null && previewObject == null)
        {
            previewObject = Instantiate(barricadePrefab);
            
            // Desativa colliders e scripts no preview
            Collider2D col = previewObject.GetComponent<Collider2D>();
            if (col != null) col.enabled = false;

            Barricade barricadeScript = previewObject.GetComponent<Barricade>();
            if (barricadeScript != null) barricadeScript.enabled = false;

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
