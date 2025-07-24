using UnityEngine;

public enum GameState
{
    Home,
    Loading,
    Playing,
    Paused,
    GameOver,
    Victory,
    Cutscene
}

public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }
    public GameState CurrentState { get; private set; }
    public SelectedCharacterData selectedCharacterData { get; private set; }
    public PlayerData selectedPlayerData;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ChangeState(GameState newState) //게임 상태 전환용 함수
    {
        // 같은 상태로 전환할 경우 무시
        if (CurrentState == newState)
            return;

        Debug.Log($"[GameController] 상태 전환: {CurrentState} → {newState}");

        // 이전 상태에서 필요한 종료 처리
        HandleStateExit(CurrentState);

        CurrentState = newState;

        // 새로운 상태에 대한 초기화 처리
        HandleStateEnter(newState);
    }
 
    private void HandleStateExit(GameState state) //특정 상태 퇴장시
    {
        switch (state)
        {
            case GameState.Playing:
                // 예: 게임 일시정지 UI 띄우기, 시간 정지 등
                break;
        }
    }

    private void HandleStateEnter(GameState state) //특정 상태 입장시
    {
        switch (state)
        {
            case GameState.Home:
                // 홈 화면 UI 활성화 등
                break;

            case GameState.Playing:
                // 게임 플레이 시작 처리 (타임스케일 등)
                break;

            case GameState.Paused:
                // 일시정지 UI 출력
                break;

            case GameState.GameOver:
                // 게임 오버 처리
                break;

            case GameState.Victory:
                // 클리어 처리
                break;

            case GameState.Loading:
                // 로딩 화면 출력
                break;

            case GameState.Cutscene:
                // 컷씬 재생 등
                break;
        }
    }

    public void SetSelectedCharacter(PlayerData data)
    {
        selectedPlayerData = data;
    }
}