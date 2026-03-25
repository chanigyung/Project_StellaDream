using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private PlayerContext context;

    // 외부 참조 호환용 미러 변수
    public bool isJumping = false;
    public bool isGrounded = true;
    public bool jumpedBefore = false;

    public void Initialize(PlayerContext ctx) // [추가]
    {
        context = ctx;
        SyncLegacyStateFromContext();
    }

    void Update()
    {
        if (context == null || context.playerInstance == null)
            return;

        if (context.jumpPressed && context.isGrounded)
        {
            context.isJumping = true; 
            context.jumpedBefore = true; 
            context.animator?.PlayJump();
        }

        if (!context.isGrounded && !context.jumpedBefore)
        {
            context.animator?.PlayJump();
            context.jumpedBefore = true; 
        }

        if (context.isGrounded)
        {
            context.jumpedBefore = false;
            context.animator?.ExitJump();
        }

        SyncLegacyStateFromContext();
    }

    void FixedUpdate()
    {
        if (!CanProcess())
            return;

        Move();        // 이동
        Jump();        // 점프 실행
        HandleAnimation();   // 애니메이션

        SyncLegacyStateFromContext();
    }

    // 좌우 이동 처리
    private void Move()
    {  
        context.unitMovement.Move(new Vector3(context.moveInput.x, 0f, 0f));
        context.unitMovement.TickMove();
    }

    //점프 작동
    private void Jump()
    {
        if (!context.isJumping)
            return;

        if (context.unitMovement.TryJump())
        {
            context.isJumping = false;
            context.isGrounded = false;
            return;
        }

        context.isJumping = false;
    }

    // 이동 방향과 입력에 따른 애니메이션 처리 함수
    private void HandleAnimation()
    {
        float inputX = context.moveInput.x;

        if (Mathf.Abs(inputX) < 0.01f)
        {
            context.animator?.PlayMove(0);
            return;
        }

        context.facingDirectionX = Mathf.Sign(inputX);

        bool isMovingLeft = inputX < 0f;
        bool isFacingLeft = context.armControl != null && context.armControl.isFacingLeft;

        context.animator?.PlayMove(isMovingLeft != isFacingLeft ? 2 : 1);
    }

    private void SyncLegacyStateFromContext() // [추가] 기존 public 필드 호환용
    {
        if (context == null)
            return;

        isJumping = context.isJumping;
        isGrounded = context.isGrounded;
        jumpedBefore = context.jumpedBefore;
    }

    // 현재 이동/입력 처리가 가능한 상태인지 판단하는 함수
    private bool CanProcess()
    {
        if (context == null || context.playerInstance == null || context.unitMovement == null)
            return false;

        if (!context.canMove)
        {
            context.unitMovement.Stop();
            context.animator?.PlayMove(0);
            context.isJumping = false;
            return false;
        }

        return true;
    }
}