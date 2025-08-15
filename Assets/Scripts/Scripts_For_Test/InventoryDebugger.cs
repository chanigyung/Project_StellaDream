using System.Collections.Generic;
using UnityEngine;

public class InventoryDebugger : MonoBehaviour
{
    public static InventoryDebugger Instance { get; private set; }

    [Header("InventoryController 데이터")]
    [SerializeField] private List<string> inventoryDataList = new();

    [Header("InventoryUIManager UI 슬롯 상태")]
    [SerializeField] private List<string> inventoryUISlotList = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        UpdateInventoryData();
        UpdateInventoryUI();
    }

    private void UpdateInventoryData()
    {
        inventoryDataList.Clear();

        if (InventoryController.Instance == null)
        {
            inventoryDataList.Add("InventoryController 없음");
            return;
        }

        var list = InventoryController.Instance.WeaponList;

        for (int i = 0; i < list.Count; i++)
        {
            string itemName = list[i]?.data?.itemName ?? "비어있음";
            inventoryDataList.Add($"[{i}] {itemName}");
        }
    }

    private void UpdateInventoryUI()
    {
        inventoryUISlotList.Clear();

        if (InventoryUIManager.Instance == null)
        {
            inventoryUISlotList.Add("InventoryUIManager 없음");
            return;
        }

        var slots = InventoryUIManager.Instance;

        for (int i = 0; i < slots.transform.childCount; i++)
        {
            var slot = slots.transform.GetChild(i).GetComponent<InventorySlot>();
            if (slot == null)
            {
                inventoryUISlotList.Add($"[{i}] 슬롯 없음");
                continue;
            }

            var weapon = slot.GetWeaponInstance();
            string name = weapon?.data?.itemName ?? "비어있음";
            inventoryUISlotList.Add($"[{i}] {name}");
        }
    }
}
