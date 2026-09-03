using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

public class AutoEventSystem : MonoBehaviour
{
    private void Awake()
    {
        // Se já existe um EventSystem na cena, não faz nada
        if (FindObjectOfType<EventSystem>() != null) return;

        // Cria o EventSystem automaticamente se estiver faltando na cena
        GameObject esObj = new GameObject("EventSystem");
        EventSystem es = esObj.AddComponent<EventSystem>();

#if ENABLE_INPUT_SYSTEM
        InputSystemUIInputModule inputModule = esObj.AddComponent<InputSystemUIInputModule>();
#else
        StandaloneInputModule inputModule = esObj.AddComponent<StandaloneInputModule>();
#endif
        Debug.Log("[AutoEventSystem] EventSystem criado automaticamente para garantir o funcionamento dos botões da UI!");
    }
}
