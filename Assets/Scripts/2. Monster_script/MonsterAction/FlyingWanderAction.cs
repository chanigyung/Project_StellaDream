using UnityEngine;

// 비행 몬스터의 배회 목적지를 정하고 Navigator에 비행 이동 명령을 요청하는 액션입니다.
public class FlyingWanderAction : IMonsterAction
{
    private const float ArriveDistance = 0.1f;

    private float retargetTimer = 0f;

    public bool CanExecute(MonsterContext context)
    {
        if (context == null) return false;
        if (!context.isFlyingMonster) return false;

        return !context.isTracing && context.canMove;
    }

    public void Execute(MonsterContext context)
    {
        MonsterData data = context.monsterInstance?.data;
        if (data == null)
            return;

        retargetTimer -= Time.deltaTime;
        if (!context.hasFlyingWanderTarget || retargetTimer <= 0f)
        {
            PickNextTarget(context, data);
            retargetTimer = Mathf.Max(0.2f, data.flyingWanderInterval);
        }

        float speedMultiplier = Mathf.Max(0f, data.flyingMoveSpeedMultiplier);
        MonsterMoveCommand command = context.navigator != null
            ? context.navigator.GetFlyingWanderCommand(context.flyingWanderTarget, ArriveDistance, speedMultiplier)
            : MonsterMoveCommand.Flying(context.flyingWanderTarget - (Vector2)context.selfTransform.position, speedMultiplier);

        if (command.reachedDestination)
        {
            if (context.navigator != null)
                context.navigator.ApplyCommand(command);
            else
                context.movement?.StopFlying();

            context.hasFlyingWanderTarget = false;
            return;
        }

        context.instance.selfSpeedMultiplier = 1f;

        if (context.navigator != null)
            context.navigator.ApplyCommand(command);
        else
            context.movement?.MoveFlying(command.flyingDirection, command.speedMultiplier);
    }

    private void PickNextTarget(MonsterContext context, MonsterData data)
    {
        context.hasFlyingWanderTarget = true;

        if (Random.value < Mathf.Clamp01(data.flyingIdleChance))
        {
            context.flyingWanderTarget = context.selfTransform.position;
            return;
        }

        Vector2 anchor = context.flyingAnchorPosition;
        Vector2 offset = Random.insideUnitCircle * Mathf.Max(0f, data.flyingWanderRadius);
        offset.y = Random.Range(data.flyingHeightOffsetRange.x, data.flyingHeightOffsetRange.y);

        context.flyingWanderTarget = anchor + offset;
    }
}
