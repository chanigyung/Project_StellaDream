using UnityEngine;

public class InGameInitializer : MonoBehaviour
{
    [Header("플레이어 생성 위치")]
    public Transform spawnPoint;

    [Header("기본 카메라 오프셋")]
    public Vector3 cameraOffset = new Vector3(0, 0, -10f);

    [Header("인게임 UI")]
    [SerializeField] private GameObject inGameUIPrefab;

    private void Start()
    {
        var playerInstance = GameController.Instance.currentPlayerInstance;
        if (playerInstance == null)
        {
            Debug.LogError("currentPlayerInstance가 null임");
        }
        else
        {
            Debug.Log("currentPlayerInstance 있음: " + playerInstance.data?.characterName);
        }

        if (playerInstance == null || playerInstance.data == null || playerInstance.data.characterPrefab == null)
        {
            Debug.LogError("[InGameInitializer] PlayerData 또는 프리팹 누락");
            return;
        }

        //플레이어, ui 프리팹 생성
        GameObject player = Instantiate(playerInstance.data.characterPrefab, spawnPoint.position, Quaternion.identity);
        GameObject ui = Instantiate(inGameUIPrefab);
        //playerController 초기화
        var controller = player.GetComponent<PlayerController>();
        //playerInstance(데이터, 스텟) 생성
        controller.Init(playerInstance);

        //캐릭터 외형 적용
        player.GetComponent<PlayerVisualApplier>()?.ApplyVisual(playerInstance.data.visualData);

        //카메라 플레이어 추적
        Camera.main.GetComponent<CameraFollow>()?.SetTarget(player.transform);

        //장착 무기 정보 복원
        PlayerWeaponManager weaponManager = player.GetComponent<PlayerWeaponManager>();

        var main = GameController.Instance.mainWeaponInstance;
        var sub = GameController.Instance.subWeaponInstance;
        var hotbarList = GameController.Instance.hotbarWeapons;

        // 데이터 컨트롤러에 핫바 무기 복원
        HotbarController.Instance.LoadWeaponList(hotbarList);
        
        if (main != null && main.data != null)
            weaponManager.EquipMainWeapon(main);

        if (sub != null && sub.data != null)
            weaponManager.EquipSubWeapon(sub);

        // UI 초기화는 자동으로 OnHotbarChanged에서 이루어짐
        if (HotbarUIManager.Instance != null)
        {
            // UI 강제 갱신 (첫 프레임에서 보장되도록)
            HotbarUIManager.Instance.UpdateSlotHighlights();
            HotbarUIManager.Instance.UpdateEquippedSlotDisplay();
        }

        if (InventoryManager.Instance != null) //인벤토리 데이터 복원
        {
            InventoryManager.Instance.LoadInventoryFromData(GameController.Instance.inventoryWeapons);
        }

        //게임 상태를 Playing으로 전환 (로딩 완료 후 최초 진입 시점)
        GameController.Instance.ChangeState(GameState.Playing);
    }
}
