using UnityEngine;
using System.Collections;

public class InGameInitializer : MonoBehaviour
{
    [Header("플레이어 생성 위치")]
    public Transform spawnPoint;

    [Header("기본 카메라 오프셋")]
    public Vector3 cameraOffset = new Vector3(0, 0, -10f);

    [Header("인게임 UI")]
    [SerializeField] private GameObject inGameUIPrefab;

    private GameObject player;

    private void Start()
    {
        InitializeImmediate();           // 즉시 처리 가능한 초기화
        StartCoroutine(InitializeDelayed()); // 한 프레임 딜레이 필요한 UI 관련 초기화
    }

    private void InitializeImmediate()
    {
        var playerInstance = GameController.Instance.currentPlayerInstance;

        if (playerInstance == null || playerInstance.data == null || playerInstance.data.characterPrefab == null)
        {
            Debug.LogError("[InGameInitializer] PlayerData 또는 프리팹 누락");
            return;
        }

        // 플레이어, UI 프리팹 생성
        player = Instantiate(playerInstance.data.characterPrefab, spawnPoint.position, Quaternion.identity);
        GameObject ui = Instantiate(inGameUIPrefab);

        // PlayerController 초기화
        var controller = player.GetComponent<PlayerController>();
        controller.Init(playerInstance);

        // 캐릭터 외형 적용
        player.GetComponent<PlayerVisualApplier>()?.ApplyVisual(playerInstance.data.visualData);

        // 카메라 추적 설정
        Camera.main.GetComponent<CameraFollow>()?.SetTarget(player.transform);
    }

    private IEnumerator InitializeDelayed()
    {
        yield return null; // UI 적용 프레임 기다림

        // 무기 장착 복원
        var weaponManager = player.GetComponent<PlayerWeaponManager>();
        var main = HotbarController.Instance.MainWeapon;
        var sub = HotbarController.Instance.SubWeapon;

        if (main != null && main.data != null)
            weaponManager.EquipMainWeapon(main);

        if (sub != null && sub.data != null)
            weaponManager.EquipSubWeapon(sub);

        // 핫바 / 인벤토리 UI 강제 갱신
        HotbarUIManager.Instance?.UpdateAllSlots();
        InventoryUIManager.Instance?.UpdateAllSlots();

        // 게임 상태 전환
        GameController.Instance.ChangeState(GameState.Playing);
    }
}
