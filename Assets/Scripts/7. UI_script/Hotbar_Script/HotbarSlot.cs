using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class HotbarSlot : MonoBehaviour, IItemSlot, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [Header("UI References")]
    public Image iconImage;
    public Image highlightImage; // 무기 장착 표시 이미지 출력용 image오브젝트
    public Image durabilityBar; //내구도 표시용 바

    [Header("Highlight Sprites")]
    public Sprite mainHighlightSprite; // 주무기 장착 표시
    public Sprite subHighlightSprite; // 보조무기 장착 표시

    public int slotIndex; // 핫바에서 슬롯 번호
    public SlotType GetSlotType() => SlotType.Hotbar;
    
    public WeaponInstance weaponInstance;
    public bool IsEmpty() => weaponInstance == null; //weaponInstace 비어있는지 체크

    //---------------------------무기 인스턴스 getter setter----------------------------//
    public void SetWeaponInstance(WeaponInstance instance)
    {
        weaponInstance = instance;
        UpdateUI();
    }

    public WeaponInstance GetWeaponInstance() => weaponInstance;

    //-------------------------------------------------------------//
    public void ClearSlot()
    {
        weaponInstance = null;
        UpdateUI();
    }

    //-------------------------UI 갱신 함수---------------------------//
    public void UpdateUI() //슬롯 상태에 따른 슬롯UI 갱신
    {
        if (weaponInstance != null && weaponInstance.data.icon != null)
        {
            iconImage.sprite = weaponInstance.data.icon;
            iconImage.color = Color.white;
        }
        else
        {
            iconImage.sprite = null;
            iconImage.color = new Color(1, 1, 1, 0); // 투명 처리
        }

        UpdateDurabilityBar();
        SetHighlight(false, false);
    } 
    
    public void UpdateDurabilityBar() // 내구도 바 갱신 함수
    {
        if (durabilityBar == null) return;

        if (weaponInstance != null && weaponInstance.isTemporary)
        {
            durabilityBar.gameObject.SetActive(true);
            durabilityBar.fillAmount = weaponInstance.GetDurabilityPercent();
        }
        else
        {
            durabilityBar.gameObject.SetActive(false);
        }
    }

    public void SetHighlight(bool isMain,bool isActive) // 장착 무기 표시 UI 갱신 함수
    {
        if (highlightImage == null) return;

        highlightImage.enabled = isActive;

        if (isActive)
        {
            highlightImage.sprite = isMain ? mainHighlightSprite : subHighlightSprite;
        }
    }

    public void SetSlot(WeaponInstance instance, int index)
    {
        slotIndex = index;
        weaponInstance = instance;
        UpdateUI();
    }

    public void OnPointerClick(PointerEventData eventData) //클릭해서 무기장착
    {
        if (weaponInstance == null) return;

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // 좌클릭 → 주무기 장착
            HotbarController.Instance.EquipMainWeaponFromSlot(slotIndex);
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            // 우클릭 → 보조무기 장착
            HotbarController.Instance.EquipSubWeaponFromSlot(slotIndex);
        }
    }

    /*--------------------------------드래그 관련 로직--------------------------------------*/
    public void OnBeginDrag(PointerEventData eventData) //드래그 시작처리, 드래그 시작 지점 등록
    {
        if (weaponInstance != null)
            DragManager.Instance.BeginDrag(this, weaponInstance);
    }

    public void OnDrag(PointerEventData eventData) 
    {
        
    }

    public void OnEndDrag(PointerEventData eventData) 
    {
        DragManager.Instance.EndDrag();
    }

    public void OnDrop(PointerEventData eventData) //무기 스왑 처리
    {
        if (!DragManager.Instance.IsDragging) return;

        var origin = DragManager.Instance.originSlot;
        if (origin == null || (object)origin == this) return;

        // 슬롯 간 무기 스왑
        DragManager.Instance.TryDropOn(this); // 슬롯 교환 처리
        HotbarController.Instance.UpdateSlotHighlights();
    }
}
