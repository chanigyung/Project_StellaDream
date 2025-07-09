using UnityEngine;

public class PlayerItemInteractor : MonoBehaviour
{
    public PlayerController playerController;
    private WeaponDrop currentHighlightedDrop;

    public float interactRange = 0.5f;
    public LayerMask itemLayer;
  
    void Update()
    {
        // 주변 모든 아이템 감지
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactRange, itemLayer); //collider 겹치는 모든 오브젝트의 배열

        if (hits.Length == 0)
        {
            ClearHighlight();
            return;
        }

        // 가장 가까운 아이템 탐색
        Collider2D nearest = hits[0]; //배열의 첫 번째 오브젝트로 초기화
        float minDist = Vector2.Distance(transform.position, nearest.transform.position);

        //배열 안에서 거리 가장 가까운 오브젝트 찾기
        foreach (var col in hits)
        {
            float dist = Vector2.Distance(transform.position, col.transform.position);
            if (dist < minDist)
            {
                nearest = col;
                minDist = dist;
            }
        }

        // 실제 무기 획득 처리
        WeaponDrop drop = nearest.GetComponentInParent<WeaponDrop>();

        if (drop != currentHighlightedDrop)
        {
            ClearHighlight();
            drop?.SetHighlight(true);
            currentHighlightedDrop = drop;
        }

        if(playerController.interactPressed)
        {
            bool addedToHotbar = TryAddToHotbar(drop.weaponInstance);
            bool addedToInventory = false;

            // 핫바에 추가 실패한 경우만 인벤토리 시도
            if (!addedToHotbar)
            {
                addedToInventory = InventoryManager.Instance.AddWeaponToInventory(drop.weaponInstance);
            }

            // 두 곳 중 하나라도 성공했을 때만 파괴
            if (addedToHotbar || addedToInventory)
            {
                Destroy(drop.gameObject);
                currentHighlightedDrop = null;
            }
            else
            {
                Debug.Log("획득 실패: 핫바/인벤토리 공간 없음");
            }
        }             
    }

    bool TryAddToHotbar(WeaponInstance weaponInstance)
    {
        var controller = HotbarController.Instance;

        for (int i = 0; i < controller.slots.Length; i++)
        {
            if (controller.slots[i].weaponInstance == null)
            {
                controller.slots[i].SetSlot(weaponInstance, i);
                // Debug.Log($"무기 {weaponInstance.data.itemName}이(가) 핫바 {i}번 슬롯에 등록됨");
                return true;
            }
        }

        Debug.Log("핫 바에 자리 없음. 인벤토리 획득 시도");
        return false;
    }

    // 획득가능범위 테스트용 코드
    // void OnDrawGizmosSelected()
    // {
    //     Gizmos.color = Color.yellow;
    //     Gizmos.DrawWireSphere(transform.position, interactRange);
    // }

    void ClearHighlight()
    {
        if (currentHighlightedDrop != null)
        {
            currentHighlightedDrop.SetHighlight(false);
            currentHighlightedDrop = null;
        }
    }
}
