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
        Debug.Log("✅ InGameInitializer.Start() 호출");

        var playerInstance = GameController.Instance.currentPlayerInstance;
        if (playerInstance == null)
        {
            Debug.LogError("❌ currentPlayerInstance가 null임");
        }
        else
        {
            Debug.Log("✅ currentPlayerInstance 있음: " + playerInstance.data?.characterName);
        }

        if (playerInstance == null || playerInstance.data == null || playerInstance.data.characterPrefab == null)
        {
            Debug.LogError("[InGameInitializer] PlayerData 또는 프리팹 누락");
            return;
        }

        //플레이어 프리팹 오브젝트 생성
        GameObject player = Instantiate(playerInstance.data.characterPrefab, spawnPoint.position, Quaternion.identity);
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
        
        if (main != null && main.data != null)
            weaponManager.EquipMainWeapon(main);

        if (sub != null && sub.data != null)
            weaponManager.EquipSubWeapon(sub);

        //인게임 UI 프리팹 생성
        GameObject ui = Instantiate(inGameUIPrefab);

        if (HotbarController.Instance != null) //무기 매니저 연결 및 핫바 데이터 복원
        {
            HotbarController.Instance.weaponManager = weaponManager;
            HotbarController.Instance.LoadHotbarFromData(GameController.Instance.hotbarWeapons);

            HotbarController.Instance.equippedSlotDisplay.UpdateMainSlot(main);
            HotbarController.Instance.equippedSlotDisplay.UpdateSubSlot(sub);
        }

        if (InventoryManager.Instance != null) //인벤토리 데이터 복원
        {
            InventoryManager.Instance.LoadInventoryFromData(GameController.Instance.inventoryWeapons);
        }

        //게임 상태를 Playing으로 전환 (로딩 완료 후 최초 진입 시점)
        GameController.Instance.ChangeState(GameState.Playing);
    }
}
