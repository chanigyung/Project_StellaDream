using UnityEngine;

public enum MonsterState { Idle, Move, Attack, Stun }

public class MonsterAnimator : MonoBehaviour
{
    //enum기반으로 변경
    private Animator animator;
    private MonsterState currentState;

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

    public void PlayAttack()
    {
        animator.SetTrigger("Attack");
    }
}
