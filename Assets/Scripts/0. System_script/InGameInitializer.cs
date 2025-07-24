using UnityEngine;

public class InGameInitializer : MonoBehaviour
{
    [Header("플레이어 생성 위치")]
    public Transform spawnPoint;

    [Header("기본 카메라 오프셋")]
    public Vector3 cameraOffset = new Vector3(0, 0, -10f);

    private void Start()
    {
        var data = GameController.Instance.selectedPlayerData;

        if (data == null || data.characterPrefab == null)
        {
            Debug.LogError("[InGameInitializer] PlayerData 또는 프리팹 누락");
            return;
        }

        //플레이어 프리팹 오브젝트 생성
        GameObject player = Instantiate(data.characterPrefab, spawnPoint.position, Quaternion.identity);
        //playerController 초기화
        var controller = player.GetComponent<PlayerController>();
        //playerInstance(데이터, 스텟) 생성
        controller.Init(data);

        //캐릭터 외형 적용
        player.GetComponent<PlayerVisualApplier>()?.ApplyVisual(data.visualData);

        //카메라 플레이어 추적
        Camera.main.GetComponent<CameraFollow>()?.SetTarget(player.transform);
        //게임 상태 변경
        GameController.Instance.ChangeState(GameState.Playing);
    }
}
