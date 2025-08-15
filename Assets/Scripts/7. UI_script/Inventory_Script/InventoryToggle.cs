using UnityEngine;

public class InventoryToggle : MonoBehaviour
{
    [SerializeField] private GameObject inventoryPanel;

    private void Start()
    {
        inventoryPanel.SetActive(false); // 게임 시작 시 꺼둠
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            bool isNowActive = !inventoryPanel.activeSelf;
            inventoryPanel.SetActive(isNowActive);

            // 인벤토리 열릴 때마다 UI 갱신
            if (isNowActive && InventoryUIManager.Instance != null)
            {
                InventoryUIManager.Instance.UpdateAllSlots();
            }
        }
    }
}

