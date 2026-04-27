using UnityEngine;

// 지상 몬스터의 배회 방향을 정하고 Navigator에 이동 명령을 요청하는 액션입니다.
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

        MonsterMoveCommand command = context.navigator != null
            ? context.navigator.GetWanderCommand(moveDirection)
            : MonsterMoveCommand.Ground(moveDirection.x);

        moveDirection = command.shouldStop
            ? Vector3.zero
            : (command.groundDirectionX < 0f ? Vector3.left : Vector3.right);

        if (command.shouldStop)
        {
            if (context.navigator != null)
                context.navigator.ApplyCommand(command);
            else
                context.movement?.Stop();

            return;
        }

        context.instance.selfSpeedMultiplier = 1f;

        if (context.navigator != null)
            context.navigator.ApplyCommand(command);
        else
            context.movement?.Move(moveDirection);
    }
}
