using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class TowerSpot : MonoBehaviour
{
    [Header("Construção Padrão (Usada se não houver Menu)")]
    [SerializeField] private GameObject defaultTowerPrefab;
    [SerializeField] private int defaultBuildCost = 70;

    [Header("Feedback Visual")]
    [SerializeField] private SpriteRenderer spotRenderer;
    [SerializeField] private Color hoverColor = new Color(0.7f, 1f, 0.7f, 1f);
    private Color originalColor = Color.white;

    private GameObject builtTower;
    private Camera mainCam;

    public bool IsOccupied => builtTower != null;

    private void Awake()
    {
        if (spotRenderer == null) spotRenderer = GetComponent<SpriteRenderer>();
        if (spotRenderer != null) originalColor = spotRenderer.color;
        mainCam = Camera.main;

        // Garante que haja um colisor 2D para detectar os cliques do mouse
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            CircleCollider2D circleCol = gameObject.AddComponent<CircleCollider2D>();
            circleCol.radius = 0.55f;
            circleCol.isTrigger = true;
        }
    }

    private void Update()
    {
        // Detecção com New Input System para cliques
        if (IsLeftMouseClicked())
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

            Vector3 mousePos = GetMouseWorldPos();
            Collider2D col = GetComponent<Collider2D>();
            if (col != null && col.OverlapPoint(mousePos))
            {
                HandleClick();
            }
        }
    }

    private void OnMouseEnter()
    {
        if (!IsOccupied && spotRenderer != null)
        {
            spotRenderer.color = hoverColor;
        }
    }

    private void OnMouseExit()
    {
        if (!IsOccupied && spotRenderer != null)
        {
            spotRenderer.color = originalColor;
        }
    }

    private void OnMouseDown()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
        HandleClick();
    }

    private void HandleClick()
    {
        if (!IsOccupied)
        {
            // Se o menu de escolha de 3 torres existir, abre o menu
            if (TowerBuildMenuUI.Instance != null)
            {
                TowerBuildMenuUI.Instance.OpenMenu(this);
            }
            else
            {
                BuildTower(defaultTowerPrefab, defaultBuildCost);
            }
        }
        else
        {
            // Fecha o menu de construção se estiver aberto
            if (TowerBuildMenuUI.Instance != null)
            {
                TowerBuildMenuUI.Instance.CloseMenu();
            }

            // Seleciona a torre construída para ver os upgrades no painel
            Tower tower = builtTower.GetComponent<Tower>();
            if (tower != null)
            {
                tower.SelectTower();
            }
        }
    }

    public bool BuildTower(GameObject towerPrefabToBuild, int cost)
    {
        if (IsOccupied)
        {
            Debug.LogWarning("Este ponto já possui uma torre construída!");
            return false;
        }

        if (towerPrefabToBuild == null)
        {
            Debug.LogWarning("Nenhum prefab de torre foi definido para este TowerSpot!");
            return false;
        }

        if (CoinManager.Instance != null && CoinManager.Instance.SpendCoins(cost))
        {
            builtTower = Instantiate(towerPrefabToBuild, transform.position, Quaternion.identity);

            // Oculta ou diminui a opacidade do círculo branco do spot
            if (spotRenderer != null)
            {
                Color c = originalColor;
                c.a = 0.15f;
                spotRenderer.color = c;
            }

            Tower tower = builtTower.GetComponent<Tower>();
            if (tower != null)
            {
                tower.SelectTower();
            }

            Debug.Log($"Torre construída com sucesso no ponto por {cost} moedas!");
            return true;
        }

        Debug.LogWarning($"Moedas insuficientes para construir a torre! Custo: {cost}");
        return false;
    }

    private bool IsLeftMouseClicked()
    {
#if ENABLE_INPUT_SYSTEM
        return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
#else
        return Input.GetMouseButtonDown(0);
#endif
    }

    private Vector3 GetMouseWorldPos()
    {
        if (mainCam == null) mainCam = Camera.main;
        if (mainCam == null) return Vector3.zero;

        Vector3 screenPos;
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            Vector2 m = Mouse.current.position.ReadValue();
            screenPos = new Vector3(m.x, m.y, 0);
        }
        else screenPos = Input.mousePosition;
#else
        screenPos = Input.mousePosition;
#endif
        screenPos.z = -mainCam.transform.position.z;
        Vector3 world = mainCam.ScreenToWorldPoint(screenPos);
        world.z = 0;
        return world;
    }
}
