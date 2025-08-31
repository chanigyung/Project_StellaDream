using UnityEngine;

public class WanderAction : IMonsterAction
{
    private Vector3 moveDirection = Vector3.zero;
    private float directionChangeTimer = 0f;

    public bool CanExecute(MonsterContext context)
    {
        return !context.isPlayerDetected && !context.isStunned;
    }

    public void Execute(MonsterContext context)
    {
        directionChangeTimer -= Time.deltaTime;

        if (directionChangeTimer <= 0f)
        {
            int choice = Random.Range(0, 3); // 0: 정지, 1: 왼쪽, 2: 오른쪽
            moveDirection = choice switch
            {
                0 => Vector3.zero,
                1 => Vector3.left,
                _ => Vector3.right,
            };
            directionChangeTimer = 3f;
        }

        var animator = context.selfTransform.GetComponent<MonsterAnimator>();

        if (moveDirection == Vector3.zero)
        {
            animator?.PlayMoving(false);
        }
        else
        {
            var move = context.selfTransform.GetComponent<MonsterMovement>();
            move?.ManualMove(moveDirection);
            animator?.PlayMoving(true);
        }
    }
}