using UnityEngine;
using UnityEngine.UI;

public class UIFixRaycasts : MonoBehaviour
{
    private void Awake()
    {
        // Se este GameObject tiver uma imagem totalmente transparente (alpha 0), desativa o Raycast Target
        Image img = GetComponent<Image>();
        if (img != null && img.color.a <= 0.01f)
        {
            img.raycastTarget = false;
            Debug.Log($"[UIFixRaycasts] Raycast Target desativado automaticamente no painel transparente '{gameObject.name}' para não bloquear cliques!");
        }
    }
}
