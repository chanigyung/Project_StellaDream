using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : UnitController
{
    public static PlayerController Instance { get; private set;}
    public PlayerData playerData;

    private PlayerContext context;
    public PlayerContext Context => context;

    public Vector2 moveInput => context != null ? context.moveInput : Vector2.zero;
    public bool jumpPressed => context != null && context.jumpPressed;
    public int mouseButton => context != null ? context.mouseButton : -1;
    public bool interactPressed => context != null && context.interactPressed;

    public KeyCode jumpKey = KeyCode.W;
    public KeyCode actionKey = KeyCode.F;

    //데이터 기반, 최초 실행시
    public void Init(PlayerData data)
    {
        if (data == null)
        {
            Debug.LogError("PlayerData가 Init에 전달되지 않았습니다!");
            return;
        }

        this.playerData = data;

        PlayerInstance instance = new PlayerInstance(data);
        Initialize(instance);
    }
     
    //인스턴스 기반, 일반적인 상황에서(씬 전환 등)
    public void Init(PlayerInstance instance)
    {
        if (instance == null || instance.data == null)
        {
            Debug.LogError("PlayerInstance가 Init에 전달되지 않았습니다!");
            return;
        }

        this.playerData = instance.data;
     
        Initialize(instance);
    }

    public override void Initialize(IUnitInstance instance) // [추가]
    {
        base.Initialize(instance);

        Instance = this;

        PlayerInstance playerInstance = instance as PlayerInstance;
        if (playerInstance == null)
        {
            Debug.LogError("PlayerController.Initialize에 PlayerInstance가 아닙니다.");
            return;
        }

        playerData = playerInstance.data;

        context = new PlayerContext();
        context.selfTransform = transform;
        context.instance = playerInstance;
        context.unitMovement = GetComponent<UnitMovement>();
        context.selfGroundPoint = GroundPoint;

        context.controller = this;
        context.movement = GetComponent<PlayerMovement>();
        context.animator = GetComponent<PlayerAnimator>();
        context.armControl = GetComponent<PlayerArmControl>();
        context.skillController = GetComponent<PlayerSkillController>();
        context.interactor = GetComponent<PlayerInteractor>();

        context.movement?.Initialize(context); // [추가]

        UnitCensor censor = GetComponentInChildren<UnitCensor>(); // [추가]
        censor?.Initialize(context); // [추가]
    }

    protected override void Update()
    {
        base.Update();

        HandleInput(); // 입력 수집
        context.UpdateContext(); // Context 상태 계산
    }

    //마우스 클릭시 호출 함수
    public int GetPressedButton()
    {
        return context != null ? context.mouseButton : -1; // [수정] context 기반으로 반환
    }

    private void HandleInput()
    {
        // 이동 입력
        context.moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), 0f);
        context.jumpPressed = Input.GetKeyDown(jumpKey);
        context.interactPressed = Input.GetKeyDown(actionKey);

        // 마우스 입력
        context.leftMouseDown = Input.GetMouseButtonDown(0);
        context.leftMouseHeld = Input.GetMouseButton(0);
        context.leftMouseUp = Input.GetMouseButtonUp(0);

        context.rightMouseDown = Input.GetMouseButtonDown(1);
        context.rightMouseHeld = Input.GetMouseButton(1);
        context.rightMouseUp = Input.GetMouseButtonUp(1);

        context.mouseButton = ResolveMouseButton();

        // 조준 방향 계산
        UpdateAim();
    }

    private void ClearInput()
    {
        // 입력 차단 시 모든 입력값 초기화
        context.moveInput = Vector2.zero;
        context.jumpPressed = false;
        context.interactPressed = false;

        context.leftMouseDown = false;
        context.leftMouseHeld = false;
        context.leftMouseUp = false;

        context.rightMouseDown = false;
        context.rightMouseHeld = false;
        context.rightMouseUp = false;

        context.mouseButton = -1;
    }

    // 마우스 위치 기반 조준 방향 계산 함수
    private void UpdateAim()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            context.mouseWorldPosition = context.selfTransform.position;
            context.aimDirection = Vector2.right;
            return;
        }

        context.mouseWorldPosition = cam.ScreenToWorldPoint(Input.mousePosition);

        Vector2 dir = context.mouseWorldPosition - context.selfTransform.position;
        if (dir.sqrMagnitude > 0.0001f)
            context.aimDirection = dir.normalized;
    }

    private int ResolveMouseButton() // [추가]
    {
        if (context.leftMouseDown || context.leftMouseUp || context.leftMouseHeld)
            return 0;

        if (context.rightMouseDown || context.rightMouseUp || context.rightMouseHeld)
            return 1;

        return -1;
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
