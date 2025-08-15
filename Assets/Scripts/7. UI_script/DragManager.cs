using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragManager : MonoBehaviour
{
    //-----------------------------변수 선언 영역--------------------------------------//
    public static DragManager Instance;

    public IItemSlot originSlot { get; private set; } //드래그시 드래그 시작한 슬롯 선언
    private SlotType originSlotType; //슬롯 타입 저장용 변수
    private WeaponInstance draggingInstance; //드래그중인 무기 인스턴스 저장할 변수

    public Image dragIcon;
    [SerializeField] private Canvas canvas;
    public RectTransform inventoryPanel;
    public RectTransform hotbarPanel;
    private List<InventorySlot> inventorySlots;

    public bool IsDragging { get; private set; } = false; //드래그중 판정용 변수

    //----------------------------슬롯 판정용 변수 및 함수-------------------------------//
    private bool droppedOnSlot = false;
    public void MarkDroppedOnSlot() => droppedOnSlot = true;

    //-----------------------------인벤토리 슬롯 getter setter----------------------------//
    public void SetInventorySlots(List<InventorySlot> slots) //InventoryUIInitializer로 생성된 인벤토리 슬롯 set해주기
    {
        inventorySlots = slots;
    }

    public List<InventorySlot> GetInventorySlots()
    {
        return inventorySlots;
    }

    //---------------------------------기본 동작---------------------------------//
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
        // dragIcon.enabled = false; //초기상태 : 아이콘 안보임
    }

    public void Update()
    {
        if (IsDragging) //드래그 중 아이템 아이콘 마우스 따라 움직이기
        {

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                Input.mousePosition,
                canvas.worldCamera,
                out Vector2 localPoint);

            dragIcon.rectTransform.anchoredPosition = localPoint;
        }
    }

    //-----------------------------------드래그 관련 로직----------------------------------//
    public void BeginDrag(IItemSlot origin, WeaponInstance instance)
    {
        originSlot = origin;
        draggingInstance = instance;
        originSlotType = origin.GetSlotType();

        dragIcon.sprite = instance.data.icon;
        dragIcon.enabled = true;
        IsDragging = true;
    }

    public void EndDrag() //드래그 끝난 순간 판정
    {
        dragIcon.enabled = false;
        IsDragging = false;

        var wm = PlayerWeaponManager.Instance;
        Vector2 pointerPos = Input.mousePosition;

        bool insideInventoryPanel = inventoryPanel.gameObject.activeInHierarchy &&
            RectTransformUtility.RectangleContainsScreenPoint(inventoryPanel, pointerPos, canvas.worldCamera);

        bool insideHotbarPanel =
            RectTransformUtility.RectangleContainsScreenPoint(hotbarPanel, pointerPos, canvas.worldCamera);

        // 슬롯 위에 정상적으로 드롭되었으면 종료
        if (droppedOnSlot)
        {
            originSlot = null;
            draggingInstance = null;
            droppedOnSlot = false;
            return;
        }

        // 슬롯 외 드롭 처리
        if (!insideInventoryPanel && !insideHotbarPanel)
        {
            if (draggingInstance == wm.mainWeaponInstance || draggingInstance == wm.subWeaponInstance)
            {
                Debug.Log("장착 중인 무기는 버릴 수 없습니다.");
            }
            else
            {
                if (originSlot.GetSlotType() == SlotType.Hotbar)
                {
                    int index = ((HotbarSlot)originSlot).slotIndex;
                    HotbarController.Instance.ClearWeaponAt(index);
                }
                else
                {
                    int index = ((InventorySlot)originSlot).slotIndex;
                    InventoryController.Instance.SetWeaponAt(index, null);
                }

                Debug.Log("아이템을 버렸습니다.");
            }
        }
        else
        {
            Debug.Log("슬롯 외 드롭 → 무시됨");
        }

        originSlot = null;
        draggingInstance = null;
        droppedOnSlot = false;
    }

    public void TryDropOn(IItemSlot targetSlot) //다른 슬롯 위에 드랍할 경우 처리 함수
    {
        if (originSlot == null || targetSlot == null || draggingInstance == null) return;
        if (originSlot == targetSlot) return;

        var wm = PlayerWeaponManager.Instance;
        var fromWeapon = originSlot.GetWeaponInstance();
        var toWeapon = targetSlot.GetWeaponInstance();

        // Debug.Log($"fromWeapon: {fromWeapon?.GetHashCode()}");
        // Debug.Log($"toWeapon: {toWeapon?.GetHashCode()}");
        // Debug.Log($"mainWeapon: {wm.mainWeaponInstance?.GetHashCode()}");
        // Debug.Log($"subWeapon: {wm.subWeaponInstance?.GetHashCode()}");

        // Debug.Log($"[드롭] from = {fromWeapon?.data?.itemName ?? "null"}, to = {toWeapon?.data?.itemName ?? "null"}");

        // 금지 조건 1: 장착 무기를 인벤토리로 옮기려는 경우
        if ((fromWeapon == wm.mainWeaponInstance || fromWeapon == wm.subWeaponInstance) &&
            targetSlot.GetSlotType() == SlotType.Inventory)
        {
            Debug.Log("장착 중인 무기를 인벤토리로 이동할 수 없습니다.");
            return;
        }

        // 금지 조건 2: 장착 무기 위에 덮어씌우는 경우
        if (toWeapon != null && (toWeapon == wm.mainWeaponInstance || toWeapon == wm.subWeaponInstance))
        {
            Debug.Log("장착 중인 무기 위에는 덮어쓸 수 없습니다.");
            return;
        }

        SwapSlots(originSlot, targetSlot);
        MarkDroppedOnSlot();
    }
    
    private void SwapSlots(IItemSlot fromSlot, IItemSlot toSlot)
    {
        var fromWeapon = fromSlot.GetWeaponInstance();
        var toWeapon = toSlot.GetWeaponInstance();

        var fromType = fromSlot.GetSlotType();
        var toType = toSlot.GetSlotType();

        // from → to
        if (toType == SlotType.Inventory)
            InventoryController.Instance.SetWeaponAt(((InventorySlot)toSlot).slotIndex, fromWeapon);
        else
            HotbarController.Instance.SetWeaponAt(((HotbarSlot)toSlot).slotIndex, fromWeapon);

        // to → from
        if (fromType == SlotType.Inventory)
            InventoryController.Instance.SetWeaponAt(((InventorySlot)fromSlot).slotIndex, toWeapon);
        else
            HotbarController.Instance.SetWeaponAt(((HotbarSlot)fromSlot).slotIndex, toWeapon);
    }
}
