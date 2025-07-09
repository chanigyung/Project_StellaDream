using UnityEngine;
using UnityEngine.EventSystems;

public static class InputBlocker
{
    //클릭해도 커서가 UI요소 위에 있거나 드래그중이라면 클릭 판정 자체를 block한다.
    public static bool IsBlocked =>
        DragManager.Instance != null && DragManager.Instance.IsDragging
        || EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
}