using UnityEngine;

// 몬스터의 추적/배회 목적을 실제 이동 명령으로 변환하는 중앙 이동 판단 컴포넌트입니다.
public class MonsterNavigator : MonoBehaviour
{
    private const float JumpTriggerHeight = 0.8f;

    private MonsterContext context;
    private GroundPathPlanner groundPathPlanner;
    private FlyingPathPlanner flyingPathPlanner;

    public void Initialize(MonsterContext ctx)
    {
        context = ctx;
        InitializePlanners();
    }

    public MonsterMoveCommand GetTraceCommand()
    {
        if (context == null || context.target == null || !context.canMove)
            return MonsterMoveCommand.Stop(MonsterMoveType.Ground);

        float dirX = context.directionToTarget.x;
        if (Mathf.Abs(dirX) < 0.01f)
            dirX = context.facingDirectionX;

        float moveDirectionX = dirX < 0f ? -1f : 1f;
        bool shouldJump = ShouldJumpTowardTarget();
        bool directPathBlocked = context.isGrounded && !HasGroundInDirection(moveDirectionX);

        if (directPathBlocked && TryGetPlannedCommand(CreateGroundRequest(MonsterNavigationPurpose.Trace, moveDirectionX, true), out MonsterMoveCommand plannedCommand))
            return plannedCommand;

        if (directPathBlocked)
            return MonsterMoveCommand.Stop(MonsterMoveType.Ground);

        if (!shouldJump && context.isGrounded && TryGetPlannedCommand(CreateGroundRequest(MonsterNavigationPurpose.Trace, moveDirectionX, false), out plannedCommand))
            return plannedCommand;

        return MonsterMoveCommand.Ground(moveDirectionX, shouldJump);
    }

    public MonsterMoveCommand GetWanderCommand(Vector3 desiredDirection)
    {
        if (context == null || !context.canMove)
            return MonsterMoveCommand.Stop(MonsterMoveType.Ground);

        float dirX = desiredDirection.x;
        if (Mathf.Abs(dirX) < 0.01f)
            return MonsterMoveCommand.Stop(MonsterMoveType.Ground);

        float moveDirectionX = Mathf.Sign(dirX);

        if (context.isGrounded && !HasGroundInDirection(moveDirectionX))
        {
            float oppositeDirectionX = -moveDirectionX;
            if (HasGroundInDirection(oppositeDirectionX))
                return MonsterMoveCommand.Ground(oppositeDirectionX);

            return MonsterMoveCommand.Stop(MonsterMoveType.Ground);
        }

        return MonsterMoveCommand.Ground(moveDirectionX);
    }

    public MonsterMoveCommand GetFlyingTraceCommand(float stopDistance, float speedMultiplier)
    {
        if (context == null || context.target == null || !context.canMove)
            return MonsterMoveCommand.Stop(MonsterMoveType.Flying);

        Vector2 toTarget = context.target.transform.position - context.selfTransform.position;
        float safeStopDistance = Mathf.Max(0f, stopDistance);

        if (toTarget.sqrMagnitude <= safeStopDistance * safeStopDistance)
            return MonsterMoveCommand.Stop(MonsterMoveType.Flying);

        if (TryGetPlannedCommand(CreateFlyingRequest(MonsterNavigationPurpose.Trace, toTarget, speedMultiplier, false), out MonsterMoveCommand plannedCommand))
            return plannedCommand;

        return MonsterMoveCommand.Flying(toTarget, speedMultiplier);
    }

    public MonsterMoveCommand GetFlyingWanderCommand(Vector2 destination, float arriveDistance, float speedMultiplier)
    {
        if (context == null || !context.canMove)
            return MonsterMoveCommand.Stop(MonsterMoveType.Flying);

        Vector2 currentPosition = context.selfTransform.position;
        Vector2 toDestination = destination - currentPosition;
        float safeArriveDistance = Mathf.Max(0f, arriveDistance);

        if (toDestination.sqrMagnitude <= safeArriveDistance * safeArriveDistance)
        {
            MonsterMoveCommand stopCommand = MonsterMoveCommand.Stop(MonsterMoveType.Flying);
            stopCommand.reachedDestination = true;
            return stopCommand;
        }

        if (TryGetPlannedCommand(CreateFlyingRequest(MonsterNavigationPurpose.Wander, toDestination, speedMultiplier, false), out MonsterMoveCommand plannedCommand))
            return plannedCommand;

        return MonsterMoveCommand.Flying(toDestination, speedMultiplier);
    }

    public void ApplyCommand(MonsterMoveCommand command)
    {
        if (context == null || context.movement == null)
            return;

        if (command.moveType == MonsterMoveType.Flying)
        {
            ApplyFlyingCommand(command);
            return;
        }

        ApplyGroundCommand(command);
    }

    private void ApplyGroundCommand(MonsterMoveCommand command)
    {
        if (command.shouldStop)
        {
            context.movement.Stop();
            return;
        }

        if (command.shouldJump)
            context.movement.TryJump();

        if (Mathf.Abs(command.groundDirectionX) < 0.01f)
        {
            context.movement.Stop();
            return;
        }

        Vector3 moveDirection = command.groundDirectionX < 0f ? Vector3.left : Vector3.right;
        context.movement.Move(moveDirection);
    }

    private void ApplyFlyingCommand(MonsterMoveCommand command)
    {
        if (command.shouldStop || command.flyingDirection.sqrMagnitude <= 0.0001f)
        {
            context.movement.StopFlying();
            return;
        }

        context.movement.MoveFlying(command.flyingDirection, command.speedMultiplier);
    }

    private bool ShouldJumpTowardTarget()
    {
        if (!context.hasWallAhead || context.selfGroundPoint == null || context.target == null)
            return false;

        UnitController targetUnit = context.target.GetComponent<UnitController>();
        if (targetUnit == null)
            return false;

        float deltaY = targetUnit.GroundPoint.position.y - context.selfGroundPoint.position.y;
        return deltaY > JumpTriggerHeight;
    }

    private void InitializePlanners()
    {
        if (groundPathPlanner == null)
        {
            groundPathPlanner = GetComponent<GroundPathPlanner>();
        }

        if (flyingPathPlanner == null)
        {
            flyingPathPlanner = GetComponent<FlyingPathPlanner>();
        }

        groundPathPlanner?.Initialize(context);
        flyingPathPlanner?.Initialize(context);
    }

    private MonsterPathRequest CreateGroundRequest(MonsterNavigationPurpose purpose, float directionX, bool directPathBlocked)
    {
        return new MonsterPathRequest
        {
            moveType = MonsterMoveType.Ground,
            purpose = purpose,
            start = context.selfTransform.position,
            destination = context.target != null ? context.target.transform.position : context.selfTransform.position,
            preferredDirectionX = Mathf.Abs(directionX) > 0.01f ? Mathf.Sign(directionX) : 0f,
            speedMultiplier = 1f,
            directPathBlocked = directPathBlocked,
            target = context.target,
        };
    }

    private MonsterPathRequest CreateFlyingRequest(MonsterNavigationPurpose purpose, Vector2 desiredDirection, float speedMultiplier, bool directPathBlocked)
    {
        return new MonsterPathRequest
        {
            moveType = MonsterMoveType.Flying,
            purpose = purpose,
            start = context.selfTransform.position,
            destination = (Vector2)context.selfTransform.position + desiredDirection,
            preferredDirectionX = Mathf.Abs(desiredDirection.x) > 0.01f ? Mathf.Sign(desiredDirection.x) : 0f,
            speedMultiplier = speedMultiplier,
            directPathBlocked = directPathBlocked,
            target = context.target,
        };
    }

    private bool TryGetPlannedCommand(MonsterPathRequest request, out MonsterMoveCommand command)
    {
        command = default;

        MonsterPathPlanner planner = request.moveType == MonsterMoveType.Flying
            ? flyingPathPlanner
            : groundPathPlanner;

        if (planner == null || !planner.CanPlan(request))
            return false;

        if (!planner.TryFindPath(request, out MonsterPathResult result))
            return false;

        if (!result.success)
            return false;

        command = result.nextCommand;
        return true;
    }

    private bool HasGroundInDirection(float directionX)
    {
        if (directionX < 0f)
            return context.hasGroundLeft;

        return context.hasGroundRight;
    }
}
