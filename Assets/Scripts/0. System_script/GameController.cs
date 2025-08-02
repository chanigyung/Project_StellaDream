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

public partial class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }
    public GameState CurrentState { get; private set; }
    public SelectedCharacterData selectedCharacterData { get; private set; }
    public PlayerData selectedPlayerData;

    [Header("전역 UI 프리팹")]
    [SerializeField] private GameObject loadingUIPrefab;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (LoadingUI.Instance == null && loadingUIPrefab != null)
        {
            GameObject ui = Instantiate(loadingUIPrefab);
            DontDestroyOnLoad(ui);
        }
    }

    public void RequestSceneLoad(string sceneName)
    {
        GameSceneLoader.Instance.LoadScene(sceneName, () => { ChangeState(GameState.Playing); });
        //sceneName의 씬을 로드한 후 매개변수 없이도 ChangeState(GameState.Playing);를 실행
    }

    public void ChangeState(GameState newState) //게임 상태 전환용 함수
    {
        // 같은 상태로 전환할 경우 무시
        if (CurrentState == newState)
            return;

        Debug.Log($"[GameController] 상태 전환: {CurrentState} → {newState}");

        HandleStateExit(CurrentState); // 이전 상태에서 필요한 종료 처리
        CurrentState = newState;
        HandleStateEnter(newState); // 새로운 상태에 대한 초기화 처리
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
                UIManager.Instance?.HideAll();
                break;

            case GameState.Playing:
                UIManager.Instance?.HideAll();
                break;

            case GameState.Paused:
                UIManager.Instance?.ShowPause();
                break;

            case GameState.GameOver:
                UIManager.Instance?.ShowGameOver();
                break;

            case GameState.Victory:
                UIManager.Instance?.ShowVictory();
                break;

            case GameState.Loading:
                break;

            case GameState.Cutscene:
                UIManager.Instance?.HideAll();
                break;
        }
    }

    public void SetSelectedCharacter(PlayerData data)
    {
        selectedPlayerData = data;
    }
}