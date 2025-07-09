using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [SerializeField] private Transform inventoryPanel;
    [SerializeField] private InventorySlot[] inventorySlots;

    private void Start()
    {
        inventorySlots = inventoryPanel.GetComponentsInChildren<InventorySlot>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
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
}
