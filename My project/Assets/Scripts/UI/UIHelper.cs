using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public static class UIHelper
{
    private static List<RaycastResult> raycastResults = new List<RaycastResult>();

    /// <summary>
    /// Retorna verdadeiro se o ponteiro do mouse estiver exatamente em cima de qualquer elemento da UI (botões, painéis, etc.)
    /// </summary>
    public static bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;

        Vector2 pointerPosition = GetPointerPosition();
        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = pointerPosition
        };

        raycastResults.Clear();
        EventSystem.current.RaycastAll(eventData, raycastResults);

        return raycastResults.Count > 0;
    }

    public static Vector2 GetPointerPosition()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            return Mouse.current.position.ReadValue();
        }
#endif
        return Input.mousePosition;
    }
}
