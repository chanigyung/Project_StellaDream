using System.Collections.Generic;
using UnityEngine;

public class InventoryUIInitializer : MonoBehaviour
{
    [Header("설정할 프리팹 및 패널")]
    [SerializeField] private GameObject inventorySlotPrefab; // 슬롯 하나의 프리팹
    [SerializeField] private Transform inventoryPanel;        // 슬롯들을 배치할 부모 패널

    private List<InventorySlot> inventorySlots = new List<InventorySlot>(); // 생성된 슬롯 저장 리스트

    //인벤토리 최초 생성
    private void Start()
    {
        int slotCount = InventoryController.Instance.Capacity;
        // 슬롯을 slotCount만큼 생성
        for (int i = 0; i < slotCount; i++)
        {
            GameObject slotObj = Instantiate(inventorySlotPrefab, inventoryPanel); // 프리팹을 패널의 자식으로 생성
            slotObj.name = $"InventorySlot_{i}"; // 슬롯 오브젝트 이름 지정 (디버깅용)

            // 슬롯 오브젝트에서 InventorySlot 컴포넌트 가져오기
            InventorySlot slot = slotObj.GetComponent<InventorySlot>();
            if (slot != null)
            {
                inventorySlots.Add(slot); // 리스트에 추가
            }
            else
            {
                Debug.LogWarning($"InventorySlot 컴포넌트를 찾을 수 없습니다: {slotObj.name}");
            }
        }

        // DragManager가 정상적으로 초기화되어 있다면 슬롯 정보 전달
        if (DragManager.Instance != null)
        {
            // 생성된 슬롯 리스트 등록
            DragManager.Instance.SetInventorySlots(inventorySlots);

            // 패널의 RectTransform을 드래그 시스템에 전달 (범위 체크용)
            DragManager.Instance.inventoryPanel = inventoryPanel.GetComponent<RectTransform>();
        }
        else
        {
            Debug.LogError("DragManager.Instance가 null입니다. 인벤토리 슬롯을 연결할 수 없습니다.");
        }

        if (InventoryUIManager.Instance != null)
        {
            // 🔥 InventoryManager에도 슬롯 리스트를 전달
            InventoryUIManager.Instance.SetSlots(inventorySlots);
        }
        else
        {
            Debug.LogError("InventoryManager.Instance가 null입니다. 인벤토리 슬롯을 연결할 수 없습니다.");
        }
    }
}