using UnityEngine;

public class ItemDrop : MonoBehaviour, IInteractable
{
    public WeaponInstance weaponInstance; // 드랍된 무기의 정보
    public SpriteRenderer iconRenderer;
    public GameObject highlightObject; // 하이라이트용 오브젝트

    //드랍된 무기 아이콘 표시해주기
    public void Initialize(WeaponInstance instance)
    {
        weaponInstance = instance;

        if (weaponInstance != null && iconRenderer != null)
        {
            iconRenderer.sprite = weaponInstance.data.icon;
        }

        highlightObject?.SetActive(false);
    }

    //현재 획득 가능한(=가장 가까운) 무기가 뭔지 표시
    public void SetHighlight(bool isOn)
    {
        if (highlightObject != null)
        {
            highlightObject.SetActive(isOn);
        }
    }

    public void Interact()
    {
        bool addedToHotbar = TryAddToHotbar(weaponInstance);
        bool addedToInventory = false;

        if (!addedToHotbar)
            addedToInventory = InventoryManager.Instance.AddWeaponToInventory(weaponInstance);

        if (addedToHotbar || addedToInventory)
            Destroy(gameObject);
        else
            Debug.Log("획득 실패: 핫바/인벤토리 공간 없음");
    }

    bool TryAddToHotbar(WeaponInstance weaponInstance)
    {
        var controller = HotbarController.Instance;

        //중복 확인
        foreach (var weapon in controller.GetWeaponList())
        {
            if (weapon == weaponInstance)
            {
                Debug.Log("이미 핫바에 등록된 무기입니다.");
                return false;
            }
        }

        int emptySlot = controller.FindFirstEmptySlot();
        if (emptySlot == -1)
        {
            Debug.Log("빈 핫바 슬롯이 없습니다.");
            return false;
        }

        controller.SetWeaponAt(emptySlot, weaponInstance);
        return true;
    }
}