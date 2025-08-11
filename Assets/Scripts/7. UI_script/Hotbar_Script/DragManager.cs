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

    public bool IsDragging { get; private set;} = false; //드래그중 판정용 변수

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
            RectTransformUtility.RectangleContainsScreenPoint(inventoryPanel, Input.mousePosition, canvas.worldCamera);
        bool insideHotbarPanel = RectTransformUtility.RectangleContainsScreenPoint(hotbarPanel, pointerPos, canvas.worldCamera);


        if (!droppedOnSlot && draggingInstance != null)
        {
            //핫바 -> 인벤토리            
            if (originSlotType == SlotType.Hotbar && insideInventoryPanel)
            {
                if (draggingInstance == wm.mainWeaponInstance || draggingInstance == wm.subWeaponInstance)
                {
                    Debug.Log("장착 중인 무기는 인벤토리 슬롯에 등록할 수 없습니다.");
                }
                else
                {
                    HotbarController.Instance.RemoveWeapon(draggingInstance); // 핫바에서 무기 제거 시 Controller 경유
                    InventoryManager.Instance.AddWeaponToInventory(draggingInstance);
                }
            }
            else if (originSlotType == SlotType.Inventory && insideInventoryPanel)
            {
                Debug.Log("인벤토리 → 인벤토리 패널: 드롭 무시됨");
            }
            else if (!insideInventoryPanel && !insideHotbarPanel)
            {
                // 완전히 UI 바깥에 드롭됨 → 버림
                if (draggingInstance == wm.mainWeaponInstance || draggingInstance == wm.subWeaponInstance)
                {
                    Debug.Log("장착 중인 무기는 버릴 수 없습니다.");
                }
                else
                {
                    HotbarController.Instance.RemoveWeapon(draggingInstance);
                    Debug.Log("아이템을 버렸습니다.");
                }
            }
            else
            {
                Debug.Log("핫바 또는 인벤토리 UI 내부 드롭 → 버리기 취소");
                // 아무것도 안 함
            }
        }

        if (!droppedOnSlot)
        {
            originSlot = null;
            draggingInstance = null;
        }
        droppedOnSlot = false;
    }

    public void TryDropOn(IItemSlot targetSlot) //다른 슬롯 위에 드랍할 경우 처리 함수
    {
        if (originSlot == null || targetSlot == null || draggingInstance == null) return;

        var from = originSlot.GetWeaponInstance();
        var to = targetSlot.GetWeaponInstance();

        var wm = PlayerWeaponManager.Instance;
        bool fromIsEquipped = (from == wm.mainWeaponInstance || from == wm.subWeaponInstance); //장착된 무기를 드래그하는중?
        bool toIsEquipped = to != null && (to == wm.mainWeaponInstance || to == wm.subWeaponInstance); //장착된 무기로 드래그하는중?

        // 장착된 무기 > 인벤토리
        if (fromIsEquipped && targetSlot.GetSlotType() == SlotType.Inventory)
        {
            Debug.Log("장착 중인 무기를 인벤토리로 이동할 수 없습니다.");
            return;
        }

        // 미장착or인벤토리 > 장착된무기
        if (toIsEquipped && targetSlot.GetSlotType() == SlotType.Hotbar)
        {
            Debug.Log("장착 중인 무기 위에는 덮어쓸 수 없습니다.");
            return;
        }
        originSlot.SetWeaponInstance(to);
        targetSlot.SetWeaponInstance(from);
        
        MarkDroppedOnSlot();
        HotbarUIManager.Instance.UpdateSlotHighlights();
    }
}
