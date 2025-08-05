using System.Collections.Generic;
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

    //게임 상태
    public GameState CurrentState { get; private set; }

    public PlayerData mainPlayerData;
    public PlayerData subPlayerData;

    //플레이어 관련 정보
    public PlayerInstance currentPlayerInstance;
    public WeaponInstance mainWeaponInstance;
    public WeaponInstance subWeaponInstance;
    public List<WeaponInstance> inventoryWeapons = new(); //인벤토리 정보
    public List<WeaponInstance> hotbarWeapons = new(); //핫바 정보

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

    //메인캐릭터 데이터 가져오기
    public void SetMainPlayer(PlayerData data)
    {
        mainPlayerData = data;
        currentPlayerInstance = new PlayerInstance(data); // 여기서 인스턴스 생성
    }

    //서브캐릭터 데이터 가져오기, 서브캐릭터 구현 후 이어서 구현
    public void SetSubPlayer(PlayerData data)
    {
        subPlayerData = data;
    }

    //핫바 및 인벤토리 정보 컨트롤러에 저장하기
    public void SetInitialWeapons(List<WeaponInstance> hotbar, List<WeaponInstance> inventory)
    {
        hotbarWeapons = new List<WeaponInstance>(hotbar);
        inventoryWeapons = new List<WeaponInstance>(inventory);
    }
}