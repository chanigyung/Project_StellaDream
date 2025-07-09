using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : UnitController
{
    public PlayerData playerData;

    public Vector2 moveInput { get; private set; }
    public bool jumpPressed { get; private set; }
    public int mouseButton { get; private set; }
    public bool interactPressed { get; private set; }

    public KeyCode jumpKey = KeyCode.W;
    public KeyCode actionKey = KeyCode.F;

    private void Awake()
    {
        if (playerData == null)
        {
            Debug.LogError("PlayerData가 할당되지 않았습니다!");
            return;
        }

        PlayerInstance instance = new PlayerInstance(playerData);
        Initialize(instance);
    }

    protected override void Update()
    {
        base.Update();

        if (IsInputBlocked()) return;

        moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), 0);
        jumpPressed = Input.GetKeyDown(jumpKey);
        interactPressed = Input.GetKeyDown(actionKey);
    }

    //마우스 클릭시 호출 함수
    public int GetPressedButton()
    {
        if (IsInputBlocked()) return -1;

        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonUp(0) || Input.GetMouseButton(0))
            return 0; // 좌클릭
        if (Input.GetMouseButtonDown(1) || Input.GetMouseButtonUp(1) || Input.GetMouseButton(1))
            return 1; // 우클릭
        return -1; // 입력 없음
    }

    private bool IsInputBlocked()
    {
        return false;
    }

    protected override void HandleDeath()
    {
        // 플레이어 전용 사망 처리 (예: 게임 오버, 리스폰 등)
        // Debug.Log("플레이어 사망 처리");
        // 예: GameUI.Instance.ShowGameOver();
    }

    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);
        // Debug.Log($"플레이어가 {damage}의 데미지를 입음!");
        // 예: 피격 애니메이션, 사운드 등 추가 가능
    }

}
