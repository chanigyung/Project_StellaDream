using UnityEngine;

public class PlayerMovement : MonoBehaviour, IMovementController
{
    private PlayerController playerController;
    private PlayerInstance playerInstance;
    private PlayerArmControl playerAC;
    private PlayerAnimator animator;
    Rigidbody2D rigid;

    public bool isJumping = false; //점프 실행하면 true로 바꿔줌. 연속 점프 방지용 변수

    public bool isGrounded = true; //바닥에 닿아있는지?
    public bool jumpedBefore = false; //점프 애니메이션 이미 재생했는지 여부. 낙하시에 점프로 인한 낙하인지, 추락인지 판별하는데 쓰임.

    //상태이상 로직용 변수
    private bool isRooted = false;
    private bool isStunned = false;
    private bool isPowerKnockbacked = false;

    void Start()
    {
        rigid = gameObject.GetComponent<Rigidbody2D>();
        animator = GetComponent<PlayerAnimator>(); //playerSprite에서 가져오기
        playerAC = GetComponent<PlayerArmControl>(); //playerAttack 컴포넌트
    }

    void Awake()
    {
        playerController = GetComponent<PlayerController>();
    }

    void Update()
    {
        if (playerController == null) return;

        if (playerInstance == null)
        {
            playerInstance = playerController.instance as PlayerInstance;
            if (playerInstance == null) return;
        }

        if (playerController.jumpPressed && isGrounded)
        { //점프 시작, 점프 구현은 뒤에
            isJumping = true; //점프 중복입력 방지
            jumpedBefore = true; //점프 애니메이션 재생 했음.
            animator.PlayJump();
        }

        if (!isGrounded && !jumpedBefore)
        {
            animator.PlayJump();
            jumpedBefore = true;
        }
    }

    void FixedUpdate()
    {
        if (isRooted || isStunned || isPowerKnockbacked)
            return;
        
        Move();
        Jump();
    }

    void Move()
    {
        float inputX = playerController.moveInput.x;
        float moveSpeed = playerInstance.GetCurrentMoveSpeed();

        Vector2 velocity = rigid.velocity;

        if (isGrounded)
        {
            //  땅 위에서는 즉각 반응
            velocity.x = inputX * moveSpeed;
        }
        else
        {
            //  공중에서는 부드럽게 보간 (자연스러운 공중 제어감 유지)
            float targetX = inputX * moveSpeed;
            velocity.x = Mathf.Lerp(velocity.x, targetX, 0.1f); // Lerp 비율은 취향에 맞게 조절
        }

        rigid.velocity = velocity;

        // 애니메이션 처리 (기존 코드 유지)
        if (inputX == 0)
        {
            animator.PlayMove(0); // Idle
        }
        else
        {
            bool isMovingLeft = inputX < 0;
            bool isFacingLeft = playerAC != null && playerAC.isFacingLeft;
            animator.PlayMove(isMovingLeft != isFacingLeft ? 2 : 1);
        }
    }

    //점프 작동
    void Jump()
    {
        if (!isJumping)
            return; //점프중이 아닐경우 탈출

        float jumpPower = playerInstance.GetCurrentJumpPower();
        Vector2 jumpVelocity = new Vector2(0, jumpPower);

        rigid.velocity = new Vector2(rigid.velocity.x, 0);
        rigid.AddForce(jumpVelocity, ForceMode2D.Impulse); //y축 방향으로만 힘 주기

        isJumping = false; //점프 한번 완료
        isGrounded = false;
    }

    private int groundContactCount = 0;

    void OnTriggerEnter2D(Collider2D other) //발 콜라이더 바닥에 닿는 트리거 켜질때
    {
        if (IsGroundLayer(other))
        {
            groundContactCount++;
            if (rigid.velocity.y <= 0f)
            {
                isGrounded = true;
                jumpedBefore = false;
                animator.ExitJump();
            }
        }
    }
    void OnTriggerExit2D(Collider2D other)
    {
        if (IsGroundLayer(other))
        {
            groundContactCount = Mathf.Max(groundContactCount - 1, 0);

            // 발 아래에 더 이상 땅이 없다면 착지 해제
            if (groundContactCount == 0)
            {
                isGrounded = false;
            }
        }
    }
    
    bool IsGroundLayer(Collider2D other)
    {
        return other.gameObject.layer == 6 || other.gameObject.layer == 7;
    }

    /*----------------------------------------상태이상 관리 로직-------------------------------------*/

    public void SetRooted(bool val) => isRooted = val;
    public void SetStunned(bool val) => isStunned = val;
    public void SetPowerKnockbacked(bool val) => isPowerKnockbacked = val;
}
