using System.Collections;
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

    private bool isPlacing = false;
    private bool canPlaceThisFrame = false;
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

        // Aguarda 1 frame após clicar no botão da UI para não colocar a barricada em cima do botão
        if (!canPlaceThisFrame) return;

        // Clique esquerdo no mapa para posicionar a barreira
        if (IsLeftMouseButtonPressed())
        {
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

            // Deixa a prévia semi-transparente
            SpriteRenderer[] srs = previewObject.GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer sr in srs)
            {
                Color c = sr.color;
                c.a = 0.5f;
                sr.color = c;
                sr.sortingOrder = 100;
            }
        }
    }

    private IEnumerator EnablePlacementNextFrame()
    {
        yield return null;
        canPlaceThisFrame = true;
    }

    private void PlaceBarricade(Vector3 position)
    {
        if (CoinManager.Instance != null && CoinManager.Instance.SpendCoins(barricadeCost))
        {
            Instantiate(barricadePrefab, position, Quaternion.identity);
            Debug.Log($"Barreira posicionada com sucesso na posição {position} por {barricadeCost} moedas!");
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
