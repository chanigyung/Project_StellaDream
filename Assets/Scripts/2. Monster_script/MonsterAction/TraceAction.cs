using UnityEngine;

public class TraceAction : IMonsterAction
{
    private const float JumpTriggerHeight = 0.8f; //점프 판정용, 추후 수정

    public bool CanExecute(MonsterContext context)
    {
        if (context == null) return false;
        if (context.traceHandler == null) return false;

        return context.isTracing || context.traceHandler.DesiredTarget != null || context.traceHandler.HasDamagedTrigger;
    }

    public void Execute(MonsterContext context)
    {
        var sensor = context.traceHandler;

        // 1) 피격 트리거 처리: 영구추적 진입
        if (sensor.DamagedTrigger())
        {
            context.isTracePermanent = true;

            // 타겟 후보가 없으면 플레이어로 강제
            GameObject forced = sensor.DesiredTarget;
            if (forced == null)
                forced = GameObject.FindWithTag("Player");

            BeginTrace(context, forced);
            context.instance.selfSpeedMultiplier = sensor.TracingSpeedMultiplier;
        }

        // 2) 센서가 고른 희망 타겟
        GameObject desired = sensor.DesiredTarget;

        if (desired != null)
        {
            // 추적 시작/갱신
            if (!context.isTracing)
                BeginTrace(context, desired);
            else
                context.target = desired;

            // 해제 대기 취소
            if (context.isTraceReleasedPending)
            {
                context.isTraceReleasedPending = false;
                context.traceReleaseTimer = 0f;
            }

            // 추적 중 속도
            context.instance.selfSpeedMultiplier = sensor.TracingSpeedMultiplier;
        }
        else
        {
            // 희망 타겟이 없어도 영구추적이면 타겟을 플레이어로 유지
            if (context.isTracing && context.isTracePermanent)
            {
                if (context.target == null)
                    context.target = GameObject.FindWithTag("Player");

                context.instance.selfSpeedMultiplier = sensor.TracingSpeedMultiplier;
            }
            // 영구추적이 아니면: 해제 대기 시작/진행
            else if (context.isTracing)
            {
                if (!context.isTraceReleasedPending)
                {
                    context.isTraceReleasedPending = true;
                    context.traceReleaseTimer = sensor.TraceReleaseDelay;

                    // 해제 대기 중엔 원래 속도로 계속 따라감(요구사항)
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

        // 3) 이동은 "추적 상태 + 타겟 존재 + 이동 가능"일 때만
        if (!context.isTracing) return;
        if (context.target == null) return;
        if (!context.canMove) return;

        float dirX = context.directionToTarget.x;
        if (Mathf.Abs(dirX) < 0.01f)
            dirX = context.facingDirectionX;

        Vector3 moveDirection = (dirX < 0f) ? Vector3.left : Vector3.right;

        if (context.isGrounded)
        {
            bool hasGround = (moveDirection == Vector3.left) ? context.hasGroundLeft : context.hasGroundRight;
            if (!hasGround)
            {
                // [추가] 애니메이션 상태는 건드리지 않고 "이동만" 멈추기
                context.movement?.Stop();
                return;
            }
        }

        context.movement?.Move(moveDirection);

        // 점프 판정(기존 로직 유지)
        if (context.hasWallAhead && context.selfGroundPoint != null)
        {
            UnitController targetUnit = context.target.GetComponent<UnitController>();
            if (targetUnit != null)
            {
                float deltaY = targetUnit.GroundPoint.position.y - context.selfGroundPoint.position.y;
                if (deltaY > JumpTriggerHeight)
                    context.movement?.TryJump();
            }
        }
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