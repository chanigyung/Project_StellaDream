using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private PlayerInstance playerInstance;
    private PlayerController playerController;
    [SerializeField] private PlayerArmControl playerAC;
    [SerializeField] private PlayerAnimator animator;
    [SerializeField] private UnitMovement unitMovement;
    [SerializeField] private PlayerCensor playerCensor;

    public bool isJumping = false; //점프 실행하면 true로 바꿔줌. 연속 점프 방지용 변수
    public bool isGrounded = true; //바닥에 닿아있는지?
    public bool jumpedBefore = false; //점프 애니메이션 이미 재생했는지 여부. 낙하시에 점프로 인한 낙하인지, 추락인지 판별하는데 쓰임.

    private void Awake()
    {
        if (playerController == null)
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

        if (playerCensor != null)
            isGrounded = playerCensor.IsGrounded;

        unitMovement?.SetGrounded(isGrounded);

        if (playerController.jumpPressed && isGrounded)
        {
            isJumping = true;
            jumpedBefore = true;
            animator.PlayJump();
        }

        if (!isGrounded && !jumpedBefore)
        {
            animator.PlayJump();
            jumpedBefore = true;
        }

        if (isGrounded)
        {
            jumpedBefore = false;
            animator.ExitJump();
        }
    }

    void FixedUpdate()
    {
        if (playerController == null) return;
        if (playerInstance == null)
            playerInstance = playerController.instance as PlayerInstance;

        if (playerInstance == null || unitMovement == null)
            return;

        unitMovement.Move(new Vector3(playerController.moveInput.x, 0f, 0f));
        unitMovement.TickMove();

        Jump();

        float inputX = playerController.moveInput.x;

        if (inputX == 0)
        {
            animator.PlayMove(0);
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
            return;

        if (unitMovement != null && unitMovement.TryJump())
        {
            isJumping = false;
            isGrounded = false;
            return;
        }

        isJumping = false;
    }
}
