using UnityEngine;

public class PlayerAnimator : UnitAnimator
{
    private PlayerController controller;
    private PlayerArmControl armControl;

    protected override void Awake()
    {
        base.Awake();
        controller = GetComponent<PlayerController>();
        armControl = GetComponent<PlayerArmControl>();
    }

    public void PlayJump()
    {
        if (IsSkillAnimationPlaying)
            return;
        
        animator.SetTrigger("doJumping");
        animator.SetBool("isJumping", true);
    }

    public void ExitJump()
    {
        if (IsSkillAnimationPlaying)
            return;

        animator.SetBool("isJumping", false);
    }

    public void PlayMove(int trigger)
    {
        if (IsSkillAnimationPlaying)
            return;

        animator.SetInteger("moveState", trigger);
    }

    protected override void RestoreBaseAnimation()
    {
        if (controller == null || controller.Context == null)
            return;

        PlayerContext context = controller.Context;

        if (!context.isGrounded)
        {
            animator.SetTrigger("doJumping");
            animator.SetBool("isJumping", true);
            return;
        }

        animator.SetBool("isJumping", false);

        float inputX = context.moveInput.x;
        if (Mathf.Abs(inputX) < 0.01f)
        {
            animator.SetInteger("moveState", 0);
            return;
        }

        bool isMovingLeft = inputX < 0f;
        bool isFacingLeft = armControl != null && armControl.isFacingLeft;

        animator.SetInteger("moveState", isMovingLeft != isFacingLeft ? 2 : 1);
    }
}