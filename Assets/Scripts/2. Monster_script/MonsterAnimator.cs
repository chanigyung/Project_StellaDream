using UnityEngine;

public class MonsterAnimator : MonoBehaviour
{
    private Animator animator;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    public void PlayMoving(bool moving)
    {
        animator.SetBool("isMoving", moving);
    }

    public void PlayTracing(bool tracing)
    {
        animator.SetBool("isTracing", tracing);
    }

    public void PlayStunned(bool stunned)
    {
        // Debug.Log($"[Stun 호출] stunned = {stunned}, Time = {Time.time}");
        animator.SetTrigger("setStun");
        animator.SetBool("isStunned", stunned);
    }

    public void PlayHit()
    {
        animator.SetTrigger("Hit");
    }
}
