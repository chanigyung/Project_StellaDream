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

        // 인벤토리 패널 내부 판정
        bool insideInventoryPanel = inventoryPanel.gameObject.activeInHierarchy &&
            RectTransformUtility.RectangleContainsScreenPoint(inventoryPanel, Input.mousePosition, canvas.worldCamera);
        // 핫 바 패널 내부 판정
        bool insideHotbarPanel = RectTransformUtility.RectangleContainsScreenPoint(hotbarPanel, pointerPos, canvas.worldCamera);

        if (!droppedOnSlot && draggingInstance != null)
        {
            //핫바 -> 인벤토리            
            if (!droppedOnSlot && draggingInstance != null)
            {
                var originType = originSlot.GetSlotType();

                // [인벤토리 → 인벤토리 or 핫바 패널 내부] : 무시
                if (originType == SlotType.Inventory && (insideInventoryPanel || insideHotbarPanel))
                {
                    Debug.Log("인벤토리 무기: 슬롯 외 드롭은 무시됨");
                }
                // [핫바 → 핫바 패널] : 무시
                else if (originType == SlotType.Hotbar && insideHotbarPanel)
                {
                    Debug.Log("핫바 무기: 슬롯 외 드롭은 무시됨");
                }
                // [핫바 → 인벤토리 패널] : 인벤토리의 첫 빈 칸에 등록
                else if (originType == SlotType.Hotbar && insideInventoryPanel)
                {
                    if (draggingInstance == wm.mainWeaponInstance || draggingInstance == wm.subWeaponInstance)
                    {
                        Debug.Log("장착 중인 무기는 인벤토리로 이동할 수 없습니다.");
                    }
                    else
                    {
                        int originIndex = ((HotbarSlot)originSlot).slotIndex;
                        HotbarController.Instance.ClearWeaponAt(originIndex);
                        InventoryManager.Instance.AddWeaponToInventory(draggingInstance);
                        Debug.Log("핫바 무기 → 인벤토리 빈 슬롯에 추가됨");
                    }
                }
                // 완전히 UI 밖으로 드롭된 경우 → 삭제 처리
                else if (!insideInventoryPanel && !insideHotbarPanel)
                {
                    if (draggingInstance == wm.mainWeaponInstance || draggingInstance == wm.subWeaponInstance)
                    {
                        Debug.Log("장착 중인 무기는 버릴 수 없습니다.");
                    }
                    else
                    {
                        if (originType == SlotType.Hotbar)
                        {
                            int originIndex = ((HotbarSlot)originSlot).slotIndex;
                            HotbarController.Instance.ClearWeaponAt(originIndex);
                        }
                        else
                        {
                            // InventoryManager.Instance.RemoveWeapon(draggingInstance);
                        }

                        Debug.Log("아이템을 버렸습니다.");
                    }
                }
                else
                {
                    Debug.Log("슬롯 외 드롭 → 무시됨");
                }
            }

            originSlot = null;
            draggingInstance = null;
            droppedOnSlot = false;
        }
    }

    public void TryDropOn(IItemSlot targetSlot) //다른 슬롯 위에 드랍할 경우 처리 함수
    {
        if (originSlot == null || targetSlot == null || draggingInstance == null) return;
        var wm = PlayerWeaponManager.Instance;

        var from = originSlot.GetWeaponInstance();
        var to = targetSlot.GetWeaponInstance();

        var fromType = originSlot.GetSlotType();
        var toType = targetSlot.GetSlotType();

        if (fromType == SlotType.Hotbar && toType == SlotType.Hotbar)
        {
            int fromIndex = ((HotbarSlot)originSlot).slotIndex;
            int toIndex = ((HotbarSlot)targetSlot).slotIndex;

            HotbarController.Instance.SwapWeapons(fromIndex, toIndex);
            MarkDroppedOnSlot();
            HotbarUIManager.Instance.UpdateSlotHighlights();
            return;
        }

        // [핫바 → 인벤토리] : 장착 중이면 불가
        if (fromType == SlotType.Hotbar && toType == SlotType.Inventory)
        {
            if (from == wm.mainWeaponInstance || from == wm.subWeaponInstance)
            {
                Debug.Log("장착 중인 무기를 인벤토리로 이동할 수 없습니다.");
                return;
            }

            originSlot.SetWeaponInstance(to);
            targetSlot.SetWeaponInstance(from);
            MarkDroppedOnSlot();
            return;
        }

        // [인벤토리 → 핫바] : 장착 중인 무기 위에는 덮어쓸 수 없음
        if (fromType == SlotType.Inventory && toType == SlotType.Hotbar)
        {
            if (to == wm.mainWeaponInstance || to == wm.subWeaponInstance)
            {
                Debug.Log("장착 중인 무기 위에는 덮어쓸 수 없습니다.");
                return;
            }

            originSlot.SetWeaponInstance(to);
            targetSlot.SetWeaponInstance(from);
            MarkDroppedOnSlot();
            HotbarUIManager.Instance.UpdateSlotHighlights();
            return;
        }

        // [인벤토리 → 인벤토리] : 항상 가능
        if (fromType == SlotType.Inventory && toType == SlotType.Inventory)
        {
            originSlot.SetWeaponInstance(to);
            targetSlot.SetWeaponInstance(from);
            MarkDroppedOnSlot();
            return;
        }
    }
}
