using UnityEngine;

// 추적 상태를 관리하고 Navigator에 지상 추적 이동 명령을 요청하는 액션입니다.
public class TraceAction : IMonsterAction
{
    public bool CanExecute(MonsterContext context)
    {
        if (context == null) return false;
        if (context.traceHandler == null) return false;

        return context.isTracing || context.traceHandler.DesiredTarget != null || context.traceHandler.HasDamagedTrigger;
    }

    public void Execute(MonsterContext context)
    {
        var sensor = context.traceHandler;

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

            if (context.isTraceReleasedPending)
            {
                context.isTraceReleasedPending = false;
                context.traceReleaseTimer = 0f;
            }

            context.instance.selfSpeedMultiplier = sensor.TracingSpeedMultiplier;
        }
        else
        {
            if (context.isTracing && context.isTracePermanent)
            {
                if (context.target == null)
                    context.target = GameObject.FindWithTag("Player");

                context.instance.selfSpeedMultiplier = sensor.TracingSpeedMultiplier;
            }
            else if (context.isTracing)
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

        MonsterMoveCommand command = context.navigator != null
            ? context.navigator.GetTraceCommand()
            : MonsterMoveCommand.Ground(context.directionToTarget.x);

        if (context.navigator != null)
        {
            context.navigator.ApplyCommand(command);
            return;
        }

        context.movement?.Move(command.groundDirectionX < 0f ? Vector3.left : Vector3.right);
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
    }
}
