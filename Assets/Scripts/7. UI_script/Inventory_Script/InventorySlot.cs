using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IItemSlot, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [SerializeField] private Image iconImage;
    private WeaponInstance weaponInstance;

    public int slotIndex;
    
    public SlotType GetSlotType() => SlotType.Inventory;
    public bool IsEmpty() => weaponInstance == null;
    //------------------------------get set 함수-------------------------------------//    
    public void SetSlot(WeaponInstance instance, int index)
    {
        slotIndex = index;
        weaponInstance = instance;
        iconImage.sprite = instance?.data?.icon;
        iconImage.enabled = instance != null;
        iconImage.color = new Color(1, 1, 1, 255);
    }
    
    public WeaponInstance GetWeaponInstance() => weaponInstance;

    //---------------------------------기본 함수-----------------------------------//      
    private void Awake()
    {
        // 초기화: 완전 투명하게 설정
        iconImage.color = new Color(1, 1, 1, 0);
    }
    
    public void ClearSlot()
    {
        weaponInstance = null;
        iconImage.sprite = null;
        iconImage.enabled = false;
    }

    //--------------------------------드래그 관련 처리--------------------------------//
    public void OnBeginDrag(PointerEventData eventData) //드래그 시작
    {
        if (weaponInstance != null)
            DragManager.Instance.BeginDrag(this, weaponInstance);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (weaponInstance == null) return;
    }

    public void OnEndDrag(PointerEventData eventData) //DragManager 호출
    {
        DragManager.Instance.EndDrag();
    }

    public void OnDrop(PointerEventData eventData)
    {
        DragManager.Instance.TryDropOn(this);
    }
}
