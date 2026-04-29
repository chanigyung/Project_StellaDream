using UnityEngine;

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

        Vector2 currentPosition = context.selfTransform.position;
        Vector2 toTarget = context.flyingWanderTarget - currentPosition;

        if (toTarget.sqrMagnitude <= ArriveDistance * ArriveDistance)
        {
            context.movement?.StopFlying();
            context.hasFlyingWanderTarget = false;
            return;
        }

        context.instance.selfSpeedMultiplier = 1f;
        context.movement?.MoveFlying(toTarget, Mathf.Max(0f, data.flyingMoveSpeedMultiplier));
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
