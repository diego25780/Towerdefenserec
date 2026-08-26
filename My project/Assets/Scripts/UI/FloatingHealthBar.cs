using UnityEngine;

public class FloatingHealthBar : MonoBehaviour
{
    [Header("Posição")]
    [SerializeField] private Vector3 offset = new Vector3(0, 0.7f, 0);

    [Header("Dimensões da Barra")]
    [SerializeField] private Vector2 barSize = new Vector2(0.8f, 0.12f);
    [SerializeField] private Color backgroundColor = new Color(0.15f, 0.15f, 0.15f, 0.85f);
    [SerializeField] private Color fillColor = new Color(0.2f, 0.9f, 0.2f, 1f);
    [SerializeField] private Color lowHealthColor = new Color(0.9f, 0.2f, 0.2f, 1f);

    private GameObject barContainer;
    private Transform fillTransform;
    private SpriteRenderer fillRenderer;
    private float maxHealth = 100f;
    private float currentHealth = 100f;

    private void Start()
    {
        CreateHealthBarSprites();
        HookIntoComponents();
    }

    private void LateUpdate()
    {
        if (barContainer != null)
        {
            // Mantém a barra posicionada acima do objeto sem rotacionar junto com ele
            barContainer.transform.position = transform.position + offset;
            barContainer.transform.rotation = Quaternion.identity;
        }
    }

    private void CreateHealthBarSprites()
    {
        // Cria um container separado
        barContainer = new GameObject("HealthBarContainer");
        barContainer.transform.position = transform.position + offset;

        // Cria sprite 1x1 branco padrão
        Texture2D texture = Texture2D.whiteTexture;
        Sprite whiteSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 1f);

        // Barra de Fundo
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(barContainer.transform);
        bgObj.transform.localPosition = Vector3.zero;
        bgObj.transform.localScale = new Vector3(barSize.x + 0.04f, barSize.y + 0.04f, 1f);
        SpriteRenderer bgRenderer = bgObj.AddComponent<SpriteRenderer>();
        bgRenderer.sprite = whiteSprite;
        bgRenderer.color = backgroundColor;
        bgRenderer.sortingOrder = 50;

        // Barra de Preenchimento (Verde)
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(barContainer.transform);
        fillObj.transform.localPosition = Vector3.zero;
        fillObj.transform.localScale = new Vector3(barSize.x, barSize.y, 1f);
        fillRenderer = fillObj.AddComponent<SpriteRenderer>();
        fillRenderer.sprite = whiteSprite;
        fillRenderer.color = fillColor;
        fillRenderer.sortingOrder = 51;

        fillTransform = fillObj.transform;
    }

    private void HookIntoComponents()
    {
        Enemy enemy = GetComponent<Enemy>();
        if (enemy != null)
        {
            maxHealth = enemy.MaxHealth;
            currentHealth = enemy.CurrentHealth;
            enemy.OnHealthChanged += UpdateHealth;
            UpdateHealth(currentHealth, maxHealth);
            return;
        }

        Guard guard = GetComponent<Guard>();
        if (guard != null)
        {
            maxHealth = guard.MaxHealth;
            currentHealth = guard.CurrentHealth;
            guard.OnHealthChanged += UpdateHealth;
            UpdateHealth(currentHealth, maxHealth);
            return;
        }

        Barricade barricade = GetComponent<Barricade>();
        if (barricade != null)
        {
            maxHealth = barricade.MaxHealth;
            currentHealth = barricade.CurrentHealth;
            barricade.OnHealthChanged += UpdateHealth;
            UpdateHealth(currentHealth, maxHealth);
            return;
        }
    }

    public void UpdateHealth(float current, float max)
    {
        currentHealth = current;
        maxHealth = max;

        if (fillTransform == null || fillRenderer == null) return;

        float ratio = Mathf.Clamp01(currentHealth / maxHealth);

        // Redimensiona o preenchimento proporcionalmente
        fillTransform.localScale = new Vector3(barSize.x * ratio, barSize.y, 1f);
        // Ajusta a posição para ancorar na esquerda
        float leftOffset = (barSize.x * (1f - ratio)) / 2f;
        fillTransform.localPosition = new Vector3(-leftOffset, 0, 0);

        // Muda de cor (verde para vermelho se estiver com pouca vida)
        fillRenderer.color = Color.Lerp(lowHealthColor, fillColor, ratio);
    }

    private void OnDestroy()
    {
        if (barContainer != null)
        {
            Destroy(barContainer);
        }
    }
}
