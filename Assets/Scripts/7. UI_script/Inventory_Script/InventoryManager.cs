using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [SerializeField] private Transform inventoryPanel;
    [SerializeField] private InventorySlot[] inventorySlots;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    public void Init()
    {
        inventorySlots = inventoryPanel.GetComponentsInChildren<InventorySlot>();
    }

    public bool AddWeaponToInventory(WeaponInstance instance)
    {
        if (inventorySlots == null)
        {
            Debug.LogError("[InventoryManager] inventorySlots가 null입니다!");
            return false;
        }

        foreach (var slot in inventorySlots)
        {
            if (slot.IsEmpty())
            {
                slot.SetWeaponInstance(instance);
                Debug.Log($"[InventoryManager] 무기 등록 완료: {instance.data.itemName}");
                return true;
            }
        }

        Debug.Log("인벤토리가 가득 찼습니다.");
        return false;
    }

    public void RemoveWeaponFromInventory(WeaponInstance instance)
    {
        foreach (var slot in inventorySlots)
        {
            if (slot.GetWeaponInstance() == instance)
            {
                slot.ClearSlot();
                return;
            }
        }
    }

    public void SetInventorySlots(InventorySlot[] slots)
    {
        inventorySlots = slots;
    }

    // 로드 및 씬전환시 인벤토리 복구용
    public void LoadInventoryFromData(List<WeaponInstance> weaponList)
    {
        if (inventorySlots == null || inventorySlots.Length == 0)
        {
            Debug.Log("[InventoryManager] inventorySlots가 초기화되지 않았습니다.");
            return;
        }

        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (i < weaponList.Count)
            {
                inventorySlots[i].SetWeaponInstance(weaponList[i]);
            }
            else
            {
                inventorySlots[i].ClearSlot();
            }
        }
    }

    //현재 인벤토리 목록 반환하기
    public List<WeaponInstance> GetWeaponList()
    {
        List<WeaponInstance> result = new();
        foreach (var slot in inventorySlots)
        {
            var weapon = slot.GetWeaponInstance();
            if (weapon != null)
                result.Add(weapon);
        }
        return result;
    }
}
