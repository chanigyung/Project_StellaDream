using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HotbarController : MonoBehaviour
{
    public static HotbarController Instance;
    public EquippedSlotDisplay equippedSlotDisplay; // 장비중인 무기 표시하는 슬롯 참조용

    public HotbarSlot[] slots;
    public PlayerWeaponManager weaponManager;
    public WeaponInstance[] initialWeaponInstance;

    public RectTransform hotbarPanel; //핫바 영역 판단

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        // Debug.Log($"HotbarController Start() 호출, 슬롯 수: {slots.Length}, 무기 데이터 수: {initialWeaponDatas.Length}");        
        for (int i = 0; i < slots.Length; i++)
        {
            if (i < initialWeaponInstance.Length)
            {
                // Debug.Log($"[세팅] 슬롯 {i}번에 {initialWeaponDatas[i]?.weaponName} 세팅");
                slots[i].SetSlot(initialWeaponInstance[i], i);
            }
            else
            {
                // Debug.Log($"[세팅] 슬롯 {i}번에 NULL 세팅");
                slots[i].SetSlot(null, i);
            }
        }
    }

    //메인 무기 장착 함수
    public void EquipMainWeaponFromSlot(int index)
    {
        if (index < 0 || index >= slots.Length) return; //시스템 오류 방지용
        WeaponInstance instance = slots[index].weaponInstance;
        if (instance == null) return;

        if (weaponManager.subWeaponInstance == instance)
        {
            var temp = weaponManager.mainWeaponInstance;
            weaponManager.EquipMainWeapon(instance);
            weaponManager.EquipSubWeapon(temp); //주무기 보조무기 스왑
            UpdateSlotHighlights();
            equippedSlotDisplay.UpdateMainSlot(instance); //주무기 장착칸 업데이트
            equippedSlotDisplay.UpdateSubSlot(temp); //보조무기 장착칸 업데이트
            Debug.Log("주무기와 보조무기 스왑됨");
            return;
        }

        weaponManager.EquipMainWeapon(instance);
        UpdateSlotHighlights();

        if (instance.data.weaponType == WeaponType.TwoHanded)
        {
            equippedSlotDisplay.UpdateMainSlot(instance);
            equippedSlotDisplay.UpdateSubSlot(instance); // 보조 슬롯도 같은 무기로 출력
        }
        else
        {
            equippedSlotDisplay.UpdateMainSlot(instance);

            if (weaponManager.subWeaponInstance != null && weaponManager.subWeaponInstance.data != null)
                equippedSlotDisplay.UpdateSubSlot(weaponManager.subWeaponInstance);
            else
                equippedSlotDisplay.UpdateSubSlot(null);
        }
    }

    //보조장비 장착 함수
    public void EquipSubWeaponFromSlot(int index)
    {
        if (index < 0 || index >= slots.Length) return; //시스템 오류 방지용
        WeaponInstance instance = slots[index].weaponInstance;
        if (instance == null) return;

        if (weaponManager.mainWeaponInstance == instance)
        {
            Debug.Log("이미 주무기에 장착된 무기입니다.");
            return;
        }

        bool equipped = weaponManager.EquipSubWeapon(instance);
        if (equipped)
        {
            UpdateSlotHighlights();
            equippedSlotDisplay.UpdateSubSlot(instance);
        }
    }

    //장착한 무기 하이라이트 표시(주무기, 보조무기 표시)
    public void UpdateSlotHighlights()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            WeaponInstance instance = slots[i].weaponInstance;

            // 무기 데이터가 없는 슬롯은 항상 하이라이트 꺼주기
            if (instance == null)
            {
                slots[i].SetHighlight(false, false);
                continue;
            }
            if (weaponManager.mainWeaponInstance == instance)
            {
                slots[i].SetHighlight(true, true); // 주무기 하이라이트
            }
            else if (weaponManager.subWeaponInstance == instance)
            {
                slots[i].SetHighlight(false, true); // 보조무기 하이라이트
            }
            else
            {
                slots[i].SetHighlight(false, false); // 아무것도 아님
            }
        }
    }

    // public void ClearSlot(int index) // 슬롯 비우기/////////////////////////////////////////////////////////////////////////수정하거나 삭제
    // {
    //     if (index < 0 || index >= slots.Length) return;
    //     slots[index].SetSlot(null, index);
    // }

    public void BreakWeaponInSlot(int slotIndex) //내구도 다한 무기아이템 파괴하기
    {
        if (slotIndex < 0 || slotIndex >= slots.Length)
        {
            Debug.LogError($"[BreakWeaponInSlot] 유효하지 않은 슬롯 인덱스: {slotIndex}");
            return;
        }

        var slot = slots[slotIndex];
        var weaponInstance = slot.weaponInstance;

        string tag = weaponInstance.GetPrimaryTag();
        var drops = MaterialDropManager.GenerateRandomMaterialDrops(tag);

        foreach (var material in drops)
        {
            // InventoryManager.Instance.Add(material); // 인벤토리 추가
            Debug.Log($"재료 획득: {material.itemName}");
        }

        var weaponManager = HotbarController.Instance.weaponManager;
        if (weaponManager == null) return;

        if (weaponManager.mainWeaponInstance == weaponInstance)
        {
            weaponManager.UnequipMainWeapon();
        }
        else if (weaponManager.subWeaponInstance == weaponInstance)
        {
            weaponManager.UnequipSubWeapon();
        }

        // 슬롯 제거
        slots[slotIndex].ClearSlot();
    }

    public int GetSlotIndexByWeapon(WeaponInstance weapon) //weapon이 있는 슬롯의 번호를 리턴
    {
        for (int i = 0; i < slots.Length; i++)
            if (slots[i].weaponInstance == weapon)
                return i;

        return -1;
    }
    
    // 로드 및 씬전환시 핫바 슬롯 복구용
    public void LoadHotbarFromData(List<WeaponInstance> weaponList)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (i < weaponList.Count)
            {
                slots[i].SetSlot(weaponList[i], i);
            }
            else
            {
                slots[i].SetSlot(null, i);
            }
        }

        UpdateSlotHighlights(); // 장착 무기 하이라이트 표시 갱신
    }
}