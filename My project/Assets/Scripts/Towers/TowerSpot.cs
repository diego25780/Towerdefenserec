using UnityEngine;
using UnityEngine.EventSystems;

public class TowerSpot : MonoBehaviour
{
    [Header("Configuração de Construção")]
    [Tooltip("Prefab da torre a ser construída neste ponto.")]
    [SerializeField] private GameObject defaultTowerPrefab;
    [SerializeField] private int buildCost = 80;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer spotRenderer;
    [SerializeField] private Color hoverColor = new Color(0.8f, 1f, 0.8f, 0.8f);
    private Color originalColor;

    private GameObject builtTower;
    public bool IsOccupied => builtTower != null;
    public int BuildCost => buildCost;

    private void Awake()
    {
        if (spotRenderer == null) spotRenderer = GetComponent<SpriteRenderer>();
        if (spotRenderer != null) originalColor = spotRenderer.color;
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
        if (spotRenderer != null)
        {
            spotRenderer.color = originalColor;
        }
    }

    private void OnMouseDown()
    {
        // Evita clicar no ponto se estiver com o mouse sobre um botão de UI
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (!IsOccupied)
        {
            BuildTower(defaultTowerPrefab, buildCost);
        }
        else
        {
            // Seleciona a torre existente para upgrades
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
            Debug.LogWarning("Nenhum prefab de torre foi definido para este ponto!");
            return false;
        }

        if (CoinManager.Instance != null && CoinManager.Instance.SpendCoins(cost))
        {
            builtTower = Instantiate(towerPrefabToBuild, transform.position, Quaternion.identity);
            
            // Oculta o marcador do ponto de construção
            if (spotRenderer != null)
            {
                Color c = originalColor;
                c.a = 0.2f;
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
}
