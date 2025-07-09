using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    private Animator animator;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    public void PlayJump()
    {
        animator.SetTrigger("doJumping");
        animator.SetBool("isJumping", true);
    }

    public void ExitJump()
    {
        animator.SetBool("isJumping", false);
    }

    public void PlayMove(int trigger)
    {
        animator.SetInteger("moveState", trigger);
    }
}