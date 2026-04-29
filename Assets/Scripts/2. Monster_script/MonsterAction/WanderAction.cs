using UnityEngine;

public class WanderAction : IMonsterAction
{
    private Vector3 moveDirection = Vector3.zero;
    private float directionChangeTimer = 0f;

    public bool CanExecute(MonsterContext context)
    {
        return !context.isTracing && context.canMove;
    }

    public void Execute(MonsterContext context)
    {
        directionChangeTimer -= Time.deltaTime;

        if (directionChangeTimer <= 0f)
        {
            int choice = Random.Range(0, 3);
            moveDirection = choice switch
            {
                0 => Vector3.zero,
                1 => Vector3.left,
                _ => Vector3.right,
            };
            directionChangeTimer = 3f;
        }

        Vector3 finalDirection = moveDirection;

        if (context.isGrounded && finalDirection != Vector3.zero)
        {
            if (finalDirection == Vector3.left && !context.hasGroundLeft)
            {
                finalDirection = context.hasGroundRight ? Vector3.right : Vector3.zero;
            }
            else if (finalDirection == Vector3.right && !context.hasGroundRight)
            {
                finalDirection = context.hasGroundLeft ? Vector3.left : Vector3.zero;
            }
        }

        // 절벽 체크로 방향이 바뀐 경우, 다음 프레임도 유지되도록 갱신
        moveDirection = finalDirection;

        if (finalDirection == Vector3.zero)
        {
            context.movement?.Stop();
            return;
        }

        context.instance.selfSpeedMultiplier = 1f;
        context.movement?.Move(finalDirection);
    }
}