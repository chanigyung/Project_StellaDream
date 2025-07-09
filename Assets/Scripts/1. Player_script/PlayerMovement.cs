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

        playerInstance = playerController?.instance as PlayerInstance;
    }

    void Awake()
    {
        playerController = GetComponent<PlayerController>();
    }

    void Update()
    {
        if (playerController == null) return;

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
        float inputX = playerController.moveInput.x; // 바라보는 X축 방향 변수.
        Vector3 moveVelocity = Vector3.zero; //움직임 벡터값 초기화
        if (inputX == 0)
        {
            animator.PlayMove(0); // Idle
        }
        else
        {
            moveVelocity = inputX < 0 ? Vector3.left : Vector3.right; //걸어가는 방향 왼쪽이면 left반환 오른쪽이면 right반환

            bool isMovingLeft = inputX < 0; //왼쪽으로 움직이면 true, 오른쪽이면 false
            bool isFacingLeft = playerAC != null && playerAC.isFacingLeft; //playerArmControl에서 가져온 바라보는(마우스)방향, 왼쪽이면 true 오른쪽이면 false

            // 방향이 다르면 moonwalk, 같으면 walk
            if (isMovingLeft != isFacingLeft) //보는방향 걷는방향이 다르면
                animator.PlayMove(2); // Moonwalk
            else //보는방향 걷는방향이 같으면
                animator.PlayMove(1); // Walk
        }
        float moveSpeed = playerInstance.GetCurrentMoveSpeed();
        transform.position += moveVelocity * moveSpeed * Time.deltaTime; //이동 계산식
    }

    //점프 작동
    void Jump()
    {
        if (!isJumping)
            return; //점프중이 아닐경우 탈출

        float jumpPower = playerInstance.GetCurrentJumpPower();
        rigid.velocity = Vector2.zero;

        Vector2 jumpVelocity = new Vector2(0, jumpPower);
        rigid.AddForce(jumpVelocity, ForceMode2D.Impulse); //y축 방향으로만 힘 주기

        isJumping = false; //점프 한번 완료
        isGrounded = false;
    }

    void OnTriggerEnter2D(Collider2D other) //발 콜라이더 바닥에 닿는 트리거 켜질때
    {
        if ((other.gameObject.layer == 6 || other.gameObject.layer == 7) && rigid.velocity.y < 0.01f) //바닥에 닿아있고 점프 힘 0일때 점프 애니메이션 종료시키기
        {
            isGrounded = true; //바닥과 닿았음
            animator.ExitJump(); //점프애니메이션 끄기
        }
        // else if (other.gameObject.layer == 12)
        // {

        // }
    }
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.layer == 6 || other.gameObject.layer == 7)
        {
            isGrounded = false; //바닥에서 떨어짐
        }
    }

    /*----------------------------------------상태이상 관리 로직-------------------------------------*/
    
    public void SetRooted(bool val) => isRooted = val;
    public void SetStunned(bool val) => isStunned = val;
    public void SetPowerKnockbacked(bool val) => isPowerKnockbacked = val;
}
