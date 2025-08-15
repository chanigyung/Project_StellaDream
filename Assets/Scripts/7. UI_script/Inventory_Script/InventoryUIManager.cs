using System.Collections.Generic;
using UnityEngine;

public class InventoryUIManager : MonoBehaviour
{
    public static InventoryUIManager Instance { get; private set; }

    [Header("슬롯 UI")]
    [SerializeField] private List<InventorySlot> inventorySlots;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    private void Start()
    {
        if (InventoryController.Instance != null)
            InventoryController.Instance.OnInventoryChanged += UpdateAllSlots;
    }

    private void OnDestroy()
    {
        if (InventoryController.Instance != null)
            InventoryController.Instance.OnInventoryChanged -= UpdateAllSlots;
    }

    public void SetSlots(List<InventorySlot> slots)
    {
        inventorySlots = slots;
    }

    // 전체 UI 슬롯 갱신
    public void UpdateAllSlots()
    {
        var weaponList = InventoryController.Instance.WeaponList;

        for (int i = 0; i < inventorySlots.Count; i++)
        {
            if (i < weaponList.Count)
                inventorySlots[i].SetSlot(weaponList[i], i);
            else
                inventorySlots[i].SetSlot(null, i);
        }
    }

    public void ClearAllSlots()
    {
        foreach (var slot in inventorySlots)
        {
            slot.ClearSlot();
        }
    }

    // 특정 슬롯 UI만 갱신
    public void UpdateSlot(int index)
    {
        if (index < 0 || index >= inventorySlots.Count) return;

        var weapon = InventoryController.Instance.WeaponList[index];
        inventorySlots[index].SetSlot(weapon, index);
    }
}
