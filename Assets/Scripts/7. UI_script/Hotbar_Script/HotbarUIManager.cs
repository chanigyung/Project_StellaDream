using System.Collections;
using UnityEngine;

public class HotbarUIManager : MonoBehaviour
{
    public static HotbarUIManager Instance { get; private set; }

    [Header("핫바 슬롯")]
    public HotbarSlot[] slots;

    [Header("장착된 무기 표시 UI")]
    public EquippedSlotDisplay equippedSlotDisplay;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // 데이터 컨트롤러가 핫바 변경될 때마다 슬롯 UI 업데이트(이벤트 구독)
        HotbarController.Instance.OnHotbarChanged += UpdateAllSlots;
    }

    private void OnDestroy()
    {
        if (HotbarController.Instance != null)
            HotbarController.Instance.OnHotbarChanged -= UpdateAllSlots;
    }

    public void UpdateAllSlots() //슬롯에 무기 출력, 하이라이트 및 장착 UI 표시
    {
        var weaponList = HotbarController.Instance.WeaponList;

        for (int i = 0; i < slots.Length; i++)
        {
            if (i < weaponList.Count)
                slots[i].SetSlot(weaponList[i], i);
            else
                slots[i].SetSlot(null, i);
        }

        UpdateSlotHighlights();
        UpdateEquippedSlotDisplay();
    }

    public void UpdateSlotHighlights() //각 슬롯에 주무기/보조무기 표시
    {
        for (int i = 0; i < slots.Length; i++)
        {
            WeaponInstance instance = slots[i].weaponInstance;

            if (instance == null)
            {
                slots[i].SetHighlight(false, false);
                continue;
            }

            if (HotbarController.Instance.MainWeapon == instance)
            {
                slots[i].SetHighlight(true, true); // 주무기 하이라이트
            }
            else if (HotbarController.Instance.SubWeapon == instance)
            {
                slots[i].SetHighlight(false, true); // 보조무기 하이라이트
            }
            else
            {
                slots[i].SetHighlight(false, false); // 미장착
            }
        }
    }

    public void UpdateEquippedSlotDisplay() //장착된 무기를 별도 아이콘 UI로 출력
    {
        var main = HotbarController.Instance.MainWeapon;
        var sub = HotbarController.Instance.SubWeapon;

        if (main != null && main.data != null && main.data.weaponType == WeaponType.TwoHanded) //양손무기면 양쪽슬롯 다 양손무기로 표시
        {
            equippedSlotDisplay.UpdateMainSlot(main);
            equippedSlotDisplay.UpdateSubSlot(main);
        }
        else
        {
            equippedSlotDisplay.UpdateMainSlot(main);
            equippedSlotDisplay.UpdateSubSlot(sub);
        }
    }

    public void UpdateDurabilityUI(WeaponInstance instance) //무기 내구도 바 개별 업데이트
    {
        int slotIndex = GetSlotIndexByWeapon(instance);
        if (slotIndex != -1)
        {
            slots[slotIndex].UpdateDurabilityBar();
        }
    }

    public int GetSlotIndexByWeapon(WeaponInstance instance)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].weaponInstance == instance)
                return i;
        }
        return -1;
    }

    public void ClearAllSlots()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].ClearSlot();
        }
    }
}
