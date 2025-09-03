using UnityEngine;

public class WanderAction : IMonsterAction
{
    private Vector3 moveDirection = Vector3.zero;
    private float directionChangeTimer = 0f;

    public bool CanExecute(MonsterContext context)
    {
        return !context.isPlayerDetected && context.canMove;
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

        if (moveDirection == Vector3.zero)
        {
            context.movement?.Stop();      
            context.animator?.PlayMoving(false);
        }
        else
        {
            context.instance.selfSpeedMultiplier = 1f;
            context.movement?.Move(moveDirection);
            context.animator?.PlayMoving(true);
        }
    }
}