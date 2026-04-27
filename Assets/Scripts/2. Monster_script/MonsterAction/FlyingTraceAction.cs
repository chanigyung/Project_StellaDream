using UnityEngine;

// 비행 몬스터의 추적 상태를 관리하고 Navigator에 비행 이동 명령을 요청하는 액션입니다.
public class FlyingTraceAction : IMonsterAction
{
    public bool CanExecute(MonsterContext context)
    {
        if (context == null) return false;
        if (!context.isFlyingMonster) return false;
        if (context.traceHandler == null) return false;

        return context.isTracing || context.traceHandler.DesiredTarget != null || context.traceHandler.HasDamagedTrigger;
    }

    public void Execute(MonsterContext context)
    {
        MonsterTraceHandler sensor = context.traceHandler;
        MonsterData data = context.monsterInstance?.data;
        if (sensor == null || data == null)
            return;

        if (sensor.DamagedTrigger())
        {
            context.isTracePermanent = true;

            GameObject forced = sensor.DesiredTarget;
            if (forced == null)
                forced = GameObject.FindWithTag("Player");

            BeginTrace(context, forced);
            context.instance.selfSpeedMultiplier = sensor.TracingSpeedMultiplier;
        }

        GameObject desired = sensor.DesiredTarget;
        if (desired != null)
        {
            if (!context.isTracing)
                BeginTrace(context, desired);
            else
                context.target = desired;

            context.isTraceReleasedPending = false;
            context.traceReleaseTimer = 0f;
            context.instance.selfSpeedMultiplier = sensor.TracingSpeedMultiplier;
        }
        else if (context.isTracing)
        {
            if (context.isTracePermanent)
            {
                if (context.target == null)
                    context.target = GameObject.FindWithTag("Player");

                context.instance.selfSpeedMultiplier = sensor.TracingSpeedMultiplier;
            }
            else
            {
                if (!context.isTraceReleasedPending)
                {
                    context.isTraceReleasedPending = true;
                    context.traceReleaseTimer = sensor.TraceReleaseDelay;
                    context.instance.selfSpeedMultiplier = sensor.PendingSpeedMultiplier;
                }
                else
                {
                    context.traceReleaseTimer -= Time.deltaTime;
                    if (context.traceReleaseTimer <= 0f)
                        EndTrace(context);
                }
            }
        }

        if (!context.isTracing) return;
        if (context.target == null) return;
        if (!context.canMove) return;

        float stopDistance = Mathf.Max(0f, data.flyingTraceStopDistance);
        float speedMultiplier = Mathf.Max(0f, data.flyingMoveSpeedMultiplier);

        MonsterMoveCommand command = context.navigator != null
            ? context.navigator.GetFlyingTraceCommand(stopDistance, speedMultiplier)
            : MonsterMoveCommand.Flying(context.target.transform.position - context.selfTransform.position, speedMultiplier);

        if (context.navigator != null)
        {
            context.navigator.ApplyCommand(command);
            return;
        }

        if (command.shouldStop)
            context.movement?.StopFlying();
        else
            context.movement?.MoveFlying(command.flyingDirection, command.speedMultiplier);
    }

    private void BeginTrace(MonsterContext context, GameObject targetObj)
    {
        context.isTracing = true;
        context.isTraceReleasedPending = false;
        context.traceReleaseTimer = 0f;
        context.target = targetObj != null ? targetObj : GameObject.FindWithTag("Player");
    }

    private void EndTrace(MonsterContext context)
    {
        context.isTracing = false;
        context.isTraceReleasedPending = false;
        context.traceReleaseTimer = 0f;
        context.instance.selfSpeedMultiplier = 1f;
        context.target = null;
        context.isTracePermanent = false;
        context.movement?.StopFlying();
    }
}
