using System.Collections.Generic;
using UnityEngine;

public class GroundTraceNavigator : MonoBehaviour, IMonsterTraceNavigator
{
    [Header("Map Segment")]
    [Tooltip("씬의 발판 구간 캐시입니다. 비워두면 씬의 MapSegment를 자동으로 찾습니다.")]
    [SerializeField] private MapSegment mapSegment;

    [Header("Local Query")]
    [Tooltip("몬스터와 타겟을 감싸는 탐색 범위의 좌우 여유 거리입니다. 값이 클수록 더 먼 중간 발판까지 경로 후보에 포함합니다.")]
    [SerializeField] private float horizontalSearchDistance = 4f;

    [Tooltip("몬스터와 타겟보다 위쪽으로 발판을 더 조회하는 거리입니다. 값이 클수록 더 높은 플랫폼까지 경로 후보에 포함합니다.")]
    [SerializeField] private float verticalSearchUpDistance = 3f;

    [Tooltip("몬스터와 타겟보다 아래쪽으로 발판을 더 조회하는 거리입니다. 값이 클수록 더 낮은 플랫폼까지 경로 후보에 포함합니다.")]
    [SerializeField] private float verticalSearchDownDistance = 3f;

    [Tooltip("타겟이 공중에 있을 때 아래쪽 착지 발판을 찾는 거리입니다. 값이 클수록 더 아래의 발판까지 타겟 발판으로 찾습니다.")]
    [SerializeField] private float targetGroundSearchDownDistance = 8f;

    [Tooltip("같은 발판 구간으로 판단할 높이 차이입니다. 값이 클수록 약간 다른 높이도 같은 높이로 봅니다.")]
    [SerializeField] private float levelHeightTolerance = 0.45f;

    [Tooltip("아래 발판으로 점프 후보를 허용할 최대 하강 높이입니다. 값이 클수록 더 낮은 발판까지 경로로 연결합니다.")]
    [SerializeField] private float lowerJumpHeightTolerance = 2f;

    [Tooltip("속도/점프력 기반 도달 판정 여유 배율입니다. 값이 클수록 더 멀거나 높은 발판을 연결합니다.")]
    [SerializeField] private float jumpReachPadding = 1.15f;

    [Header("Debug")]
    [Tooltip("몬스터 선택 시 지형 탐색 Gizmo를 표시합니다.")]
    [SerializeField] private bool drawDebugGizmos = true;

    [Tooltip("몬스터가 경로 탐색에 사용하는 통합 조회 범위 박스를 표시합니다.")]
    [SerializeField] private bool drawSearchBoundsGizmos = true;

    [Tooltip("현재 발판에서 직접 도달 가능한 착지 후보 지점을 표시합니다.")]
    [SerializeField] private bool drawCandidateGizmos = true;

    [Tooltip("조회된 발판 구간을 표시합니다. current는 초록, target은 파란색으로 표시됩니다.")]
    [SerializeField] private bool drawSegmentGizmos = true;

    [Tooltip("계산된 발판 경로를 노란색 선과 점으로 표시합니다.")]
    [SerializeField] private bool drawPathGizmos = true;

    private readonly List<LandingCandidate> directReachCandidateDebugInfos = new();
    private readonly List<MapSegment.Segment> platformSegments = new();
    private readonly List<MapSegment.Segment> pathSegments = new();
    private readonly List<MapSegment.Segment> openSegments = new();
    private readonly HashSet<MapSegment.Segment> visitedSegments = new();
    private readonly HashSet<MapSegment.Segment> settledSegments = new();
    private readonly Dictionary<MapSegment.Segment, MapSegment.Segment> previousSegments = new();
    private readonly Dictionary<MapSegment.Segment, float> segmentCosts = new();

    private bool isFollowingJumpTransition;
    private Vector3 jumpAirMoveDirection;
    private bool hasJumpTransitionDebug;
    private Vector2 jumpTakeoffDebugPoint;
    private Vector2 jumpLandingDebugPoint;

    private const float SegmentTraversalBaseCost = 10f;
    private const float HeightChangeCostMultiplier = 1.5f;
    private const float DownwardDetourPenalty = 5f;
    private const float ApproachTieTolerance = 0.05f;
    private const float SameSegmentArriveTolerance = 0.05f;
    private const float WalkConnectionTolerance = 0.18f;
    private const float FootprintEdgePadding = 0.03f;

    private MonsterContext context;
    private Rigidbody2D rigid;
    private Collider2D bodyCollider;
    private MapSegment.Segment currentSegment;
    private MapSegment.Segment targetSegment;
    private MapSegment.Segment nextSegment;
    private bool hasNextSegment;
    private Vector2 traceGoalPoint;
    private bool hasTraceGoalPoint;

    public void Initialize(MonsterContext context)
    {
        this.context = context;
        rigid = GetComponent<Rigidbody2D>();
        ResolveBodyCollider();
    }

    public MonsterTraceMovePlan CalculateMove(MonsterContext context)
    {
        MonsterContext activeContext = context ?? this.context;
        ClearDebugState();

        if (activeContext == null)
            return MonsterTraceMovePlan.Stop();

        if (activeContext.target == null || !activeContext.canMove)
            return MonsterTraceMovePlan.Stop();

        if (isFollowingJumpTransition)
        {
            if (IsFollowingJumpInAir(activeContext))
                return MonsterTraceMovePlan.Move(jumpAirMoveDirection);

            ClearJumpTransitionState();
        }

        BuildLocalPlatformSegments(activeContext);
        BuildSegmentPath(activeContext);

        return BuildTraceMovePlan(activeContext);
    }

    private MonsterTraceMovePlan BuildTraceMovePlan(MonsterContext context)
    {
        if (currentSegment == null || targetSegment == null || context.selfGroundPoint == null)
            return MonsterTraceMovePlan.Stop();

        if (currentSegment == targetSegment)
            return BuildMovePlanToX(context, GetTraceGoalX(context));

        if (!hasNextSegment)
        {
            if (hasTraceGoalPoint)
                return BuildMovePlanToX(context, traceGoalPoint.x);

            return MonsterTraceMovePlan.Stop();
        }

        SegmentTransition transition = BuildSegmentTransition(context, currentSegment, nextSegment);
        if (transition.type == SegmentTransitionType.SameLevelWalk || transition.type == SegmentTransitionType.WalkDown)
            return MonsterTraceMovePlan.Move(transition.moveDirection);

        if (IsJumpTransition(transition.type))
        {
            return BuildJumpTransitionMovePlan(context, currentSegment, nextSegment);
        }

        GetConnectionPoints(
            currentSegment,
            nextSegment,
            context,
            context.selfGroundPoint.position.x,
            out Vector2 currentConnectionPoint,
            out _
        );
        return BuildMovePlanToX(context, currentConnectionPoint.x);
    }

    private MonsterTraceMovePlan BuildMovePlanToX(MonsterContext context, float targetX)
    {
        Vector2 selfPoint = context.selfGroundPoint.position;
        float clampedTargetX = Mathf.Clamp(targetX, currentSegment.leftX, currentSegment.rightX);
        float deltaX = clampedTargetX - selfPoint.x;

        if (Mathf.Abs(deltaX) <= SameSegmentArriveTolerance)
            return MonsterTraceMovePlan.Stop();

        return MonsterTraceMovePlan.Move(deltaX > 0f ? Vector3.right : Vector3.left);
    }

    private void ClearDebugState()
    {
        directReachCandidateDebugInfos.Clear();
        platformSegments.Clear();
        pathSegments.Clear();
        openSegments.Clear();
        visitedSegments.Clear();
        settledSegments.Clear();
        previousSegments.Clear();
        segmentCosts.Clear();
        hasJumpTransitionDebug = false;
        currentSegment = null;
        targetSegment = null;
        nextSegment = null;
        hasNextSegment = false;
        traceGoalPoint = Vector2.zero;
        hasTraceGoalPoint = false;
    }

    private void ClearJumpTransitionState()
    {
        isFollowingJumpTransition = false;
        jumpAirMoveDirection = Vector3.zero;
    }

    private bool IsFollowingJumpInAir(MonsterContext activeContext)
    {
        if (!activeContext.isGrounded)
            return true;

        ResolveRigidbody();
        return rigid != null && rigid.velocity.y > 0.05f;
    }

    private void BuildLocalPlatformSegments(MonsterContext context)
    {
        ResolveMapSegment();
        if (mapSegment == null || context.selfGroundPoint == null || context.target == null)
            return;

        Vector2 selfPoint = context.selfGroundPoint.position;
        Vector2 targetPoint = GetTargetGroundPoint(context);

        mapSegment.GetSegmentsInBounds(BuildSearchBounds(selfPoint, targetPoint), platformSegments);

        TryFindCurrentSegment(context, selfPoint, out currentSegment);
        TryFindTargetSegment(targetPoint, out targetSegment);

        AddSegmentIfMissing(currentSegment);
        AddSegmentIfMissing(targetSegment);
    }

    private void ResolveMapSegment()
    {
        if (mapSegment != null)
            return;

        mapSegment = MapSegment.Instance != null ? MapSegment.Instance : FindObjectOfType<MapSegment>();
    }

    private void AddSegmentIfMissing(MapSegment.Segment segment)
    {
        if (segment != null && !platformSegments.Contains(segment))
            platformSegments.Add(segment);
    }

    private bool TryFindTargetSegment(Vector2 targetPoint, out MapSegment.Segment result)
    {
        if (mapSegment.TryFindSegmentAtPoint(targetPoint, levelHeightTolerance, out result))
            return true;

        return mapSegment.TryFindHighestSegmentBelowPoint(
            targetPoint,
            targetGroundSearchDownDistance,
            levelHeightTolerance,
            out result
        );
    }

    private Bounds BuildSearchBounds(Vector2 selfPoint, Vector2 targetPoint)
    {
        float left = Mathf.Min(selfPoint.x, targetPoint.x) - horizontalSearchDistance;
        float right = Mathf.Max(selfPoint.x, targetPoint.x) + horizontalSearchDistance;
        float bottom = Mathf.Min(selfPoint.y, targetPoint.y) - verticalSearchDownDistance;
        float top = Mathf.Max(selfPoint.y, targetPoint.y) + verticalSearchUpDistance;

        Vector3 size = new Vector3(right - left, top - bottom, 1f);
        Vector3 boxCenter = new Vector3((left + right) * 0.5f, (bottom + top) * 0.5f, 0f);
        return new Bounds(boxCenter, size);
    }

    private bool BuildSegmentPath(MonsterContext context)
    {
        if (currentSegment == null || targetSegment == null)
            return false;

        Vector2 targetPoint = GetTargetGroundPoint(context);
        if (currentSegment == targetSegment)
        {
            pathSegments.Add(currentSegment);
            SetTraceGoal(targetPoint);
            return true;
        }

        BuildReachabilityGraph(context, targetPoint);

        if (visitedSegments.Contains(targetSegment))
        {
            ReconstructPath(targetSegment);
            SelectNextSegment();
            SetTraceGoal(targetPoint);
            return true;
        }

        if (!TryFindBestReachableApproach(targetPoint, out TraceApproachGoal approachGoal))
            return false;

        ReconstructPath(approachGoal.segment);
        SelectNextSegment();
        SetTraceGoal(approachGoal.point);
        return pathSegments.Count > 0;
    }

    private void BuildReachabilityGraph(MonsterContext context, Vector2 targetPoint)
    {
        openSegments.Add(currentSegment);
        visitedSegments.Add(currentSegment);
        segmentCosts[currentSegment] = 0f;

        while (openSegments.Count > 0)
        {
            MapSegment.Segment from = PopLowestCostOpenSegment();
            settledSegments.Add(from);

            foreach (MapSegment.Segment to in platformSegments)
            {
                if (to == null || to == from || settledSegments.Contains(to))
                    continue;

                if (!CanTraverseSegment(context, from, to))
                    continue;

                if (from == currentSegment)
                    directReachCandidateDebugInfos.Add(BuildLandingCandidate(from, to));

                float newCost = segmentCosts[from] + CalculateTraversalCost(from, to, targetPoint);
                if (segmentCosts.TryGetValue(to, out float previousCost) && newCost >= previousCost)
                    continue;

                visitedSegments.Add(to);
                previousSegments[to] = from;
                segmentCosts[to] = newCost;

                if (!openSegments.Contains(to))
                    openSegments.Add(to);
            }
        }
    }

    private MapSegment.Segment PopLowestCostOpenSegment()
    {
        int bestIndex = 0;
        float bestCost = segmentCosts[openSegments[0]];

        for (int i = 1; i < openSegments.Count; i++)
        {
            float cost = segmentCosts[openSegments[i]];
            if (cost >= bestCost)
                continue;

            bestIndex = i;
            bestCost = cost;
        }

        MapSegment.Segment result = openSegments[bestIndex];
        openSegments.RemoveAt(bestIndex);
        return result;
    }

    private bool TryFindBestReachableApproach(Vector2 targetPoint, out TraceApproachGoal result)
    {
        result = default;
        MapSegment.Segment bestSegment = null;
        Vector2 bestPoint = Vector2.zero;
        float bestHeightDistance = float.MaxValue;
        float bestSegmentGap = float.MaxValue;
        float bestTargetDistance = float.MaxValue;
        float bestPathCost = float.MaxValue;

        foreach (MapSegment.Segment segment in visitedSegments)
        {
            float pathCost = segmentCosts.TryGetValue(segment, out float cost) ? cost : 0f;
            Vector2 approachPoint = GetClosestPointOnSegmentToPoint(segment, targetPoint);
            float heightDistance = GetHeightDistanceToTargetSegment(segment);
            float segmentGap = GetHorizontalGap(segment, targetSegment);
            float targetDistance = Vector2.Distance(approachPoint, targetPoint);
            if (IsBetterApproachCandidate(
                    heightDistance,
                    segmentGap,
                    targetDistance,
                    pathCost,
                    bestHeightDistance,
                    bestSegmentGap,
                    bestTargetDistance,
                    bestPathCost))
            {
                bestHeightDistance = heightDistance;
                bestSegmentGap = segmentGap;
                bestTargetDistance = targetDistance;
                bestPathCost = pathCost;
                bestSegment = segment;
                bestPoint = approachPoint;
            }
        }

        if (bestSegment == null)
            return false;

        result = new TraceApproachGoal
        {
            segment = bestSegment,
            point = bestPoint
        };
        return true;
    }

    private bool IsBetterApproachCandidate(
        float heightDistance,
        float segmentGap,
        float targetDistance,
        float pathCost,
        float bestHeightDistance,
        float bestSegmentGap,
        float bestTargetDistance,
        float bestPathCost
    )
    {
        if (heightDistance < bestHeightDistance - ApproachTieTolerance)
            return true;

        if (heightDistance > bestHeightDistance + ApproachTieTolerance)
            return false;

        if (segmentGap < bestSegmentGap - ApproachTieTolerance)
            return true;

        if (segmentGap > bestSegmentGap + ApproachTieTolerance)
            return false;

        if (targetDistance < bestTargetDistance - ApproachTieTolerance)
            return true;

        if (targetDistance > bestTargetDistance + ApproachTieTolerance)
            return false;

        return pathCost < bestPathCost;
    }

    private float GetHeightDistanceToTargetSegment(MapSegment.Segment segment)
    {
        return targetSegment != null ? Mathf.Abs(segment.y - targetSegment.y) : 0f;
    }

    private void ReconstructPath(MapSegment.Segment goal)
    {
        pathSegments.Clear();

        MapSegment.Segment current = goal;
        while (current != null)
        {
            pathSegments.Add(current);

            if (current == currentSegment)
                break;

            if (!previousSegments.TryGetValue(current, out current))
                break;
        }

        pathSegments.Reverse();
    }

    private void SelectNextSegment()
    {
        if (pathSegments.Count <= 1)
            return;

        nextSegment = pathSegments[1];
        hasNextSegment = nextSegment != null;
    }

    private void SetTraceGoal(Vector2 point)
    {
        traceGoalPoint = point;
        hasTraceGoalPoint = true;
    }

    private float GetTraceGoalX(MonsterContext context)
    {
        return hasTraceGoalPoint ? traceGoalPoint.x : GetTargetGroundPoint(context).x;
    }

    private Vector2 GetTargetGroundPoint(MonsterContext context)
    {
        UnitController targetUnit = context.target.GetComponent<UnitController>();
        if (targetUnit != null)
            return targetUnit.GroundPoint.position;

        return context.target.transform.position;
    }

    private LandingCandidate BuildLandingCandidate(MapSegment.Segment from, MapSegment.Segment to)
    {
        Vector2 landingPoint = GetLandingPoint(from, to);
        float heightDelta = to.y - from.y;
        LandingCandidateType type = Mathf.Abs(heightDelta) <= levelHeightTolerance
            ? LandingCandidateType.SameLevel
            : heightDelta > 0f
                ? LandingCandidateType.Upper
                : LandingCandidateType.Lower;

        return new LandingCandidate
        {
            position = landingPoint,
            type = type
        };
    }

    private bool CanTraverseSegment(MonsterContext context, MapSegment.Segment from, MapSegment.Segment to)
    {
        if (context.instance == null)
            return false;

        float heightDelta = to.y - from.y;
        if (heightDelta < -lowerJumpHeightTolerance)
            return false;

        if (!TryCalculateJumpHorizontalDistance(context, from, to, out float maxHorizontalDistance))
            return false;

        return GetHorizontalGap(from, to) <= maxHorizontalDistance;
    }

    private bool TryCalculateJumpHorizontalDistance(
        MonsterContext context,
        MapSegment.Segment from,
        MapSegment.Segment to,
        out float maxHorizontalDistance
    )
    {
        maxHorizontalDistance = 0f;

        if (context.instance == null)
            return false;

        float heightDelta = to.y - from.y;
        if (heightDelta < -lowerJumpHeightTolerance)
            return false;

        ResolveRigidbody();

        float jumpPower = context.instance.GetCurrentJumpPower();
        float moveSpeed = context.instance.GetCurrentMoveSpeed();
        float mass = rigid != null ? Mathf.Max(0.0001f, rigid.mass) : 1f;
        float gravityScale = rigid != null ? Mathf.Max(0.0001f, rigid.gravityScale) : 1f;
        float gravity = Mathf.Abs(Physics2D.gravity.y * gravityScale);
        float initialJumpVelocity = jumpPower / mass;

        if (jumpPower <= 0f || moveSpeed <= 0f || gravity <= 0f)
            return false;

        float maxJumpHeight = (initialJumpVelocity * initialJumpVelocity) / (2f * gravity);
        if (heightDelta > maxJumpHeight * jumpReachPadding)
            return false;

        float fallHeight = Mathf.Max(0f, maxJumpHeight - heightDelta);
        float timeUp = initialJumpVelocity / gravity;
        float timeDown = Mathf.Sqrt((2f * fallHeight) / gravity);
        float airTime = timeUp + timeDown;
        maxHorizontalDistance = moveSpeed * airTime * jumpReachPadding;
        return true;
    }

    private float CalculateTraversalCost(MapSegment.Segment from, MapSegment.Segment to, Vector2 targetPoint)
    {
        float heightDelta = to.y - from.y;
        float cost = SegmentTraversalBaseCost
            + GetHorizontalGap(from, to)
            + (Mathf.Abs(heightDelta) * HeightChangeCostMultiplier);

        bool targetIsAbove = targetPoint.y > from.y + levelHeightTolerance;
        bool movingDown = to.y < from.y - levelHeightTolerance;
        if (targetIsAbove && movingDown)
            cost += DownwardDetourPenalty;

        return cost;
    }

    private void ResolveRigidbody()
    {
        if (rigid == null)
            rigid = GetComponent<Rigidbody2D>();
    }

    private void ResolveBodyCollider()
    {
        if (bodyCollider != null)
            return;

        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
        foreach (Collider2D candidate in colliders)
        {
            if (candidate == null || candidate.isTrigger)
                continue;

            bodyCollider = candidate;
            return;
        }

        bodyCollider = GetComponent<Collider2D>();
    }

    private bool TryFindCurrentSegment(MonsterContext activeContext, Vector2 selfPoint, out MapSegment.Segment result)
    {
        ResolveBodyCollider();

        if (bodyCollider == null)
            return mapSegment.TryFindSegmentAtPoint(selfPoint, levelHeightTolerance, out result);

        Bounds bounds = bodyCollider.bounds;
        float leftX = Mathf.Min(bounds.min.x, bounds.max.x) - FootprintEdgePadding;
        float rightX = Mathf.Max(bounds.min.x, bounds.max.x) + FootprintEdgePadding;
        float y = activeContext.selfGroundPoint != null ? activeContext.selfGroundPoint.position.y : selfPoint.y;

        if (mapSegment.TryFindSegmentOverlappingFootprint(leftX, rightX, y, levelHeightTolerance, out result))
            return true;

        return mapSegment.TryFindSegmentAtPoint(selfPoint, levelHeightTolerance, out result);
    }

    private float GetHorizontalGap(MapSegment.Segment from, MapSegment.Segment to)
    {
        if (from.rightX >= to.leftX && to.rightX >= from.leftX)
            return 0f;

        if (from.rightX < to.leftX)
            return to.leftX - from.rightX;

        return from.leftX - to.rightX;
    }

    private Vector2 GetClosestPointOnSegmentToPoint(MapSegment.Segment segment, Vector2 point)
    {
        float x = Mathf.Clamp(point.x, segment.leftX, segment.rightX);
        return new Vector2(x, segment.y);
    }

    private Vector2 GetLandingPoint(MapSegment.Segment from, MapSegment.Segment to)
    {
        if (from.rightX >= to.leftX && to.rightX >= from.leftX)
        {
            float overlapLeft = Mathf.Max(from.leftX, to.leftX);
            float overlapRight = Mathf.Min(from.rightX, to.rightX);
            return new Vector2((overlapLeft + overlapRight) * 0.5f, to.y);
        }

        if (from.rightX < to.leftX)
            return new Vector2(to.leftX, to.y);

        return new Vector2(to.rightX, to.y);
    }

    private Color GetCandidateColor(LandingCandidateType type)
    {
        return type switch
        {
            LandingCandidateType.Upper => Color.cyan,
            LandingCandidateType.SameLevel => Color.magenta,
            LandingCandidateType.Lower => Color.blue,
            _ => Color.white
        };
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawDebugGizmos)
            return;

        if (drawSearchBoundsGizmos)
            DrawSearchBounds();

        if (drawSegmentGizmos)
            DrawQueriedSegments();

        if (drawPathGizmos)
            DrawPath();

        if (drawPathGizmos)
            DrawJumpTransitionDebug();

        if (drawSegmentGizmos)
            DrawCurrentAndTargetSegments();

        if (drawCandidateGizmos)
            DrawCandidatePoints();
    }

    private void DrawSearchBounds()
    {
        if (context == null || context.selfGroundPoint == null || context.target == null)
            return;

        Bounds bounds = BuildSearchBounds(context.selfGroundPoint.position, GetTargetGroundPoint(context));
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(bounds.center, bounds.size);
    }

    private void DrawQueriedSegments()
    {
        foreach (MapSegment.Segment segment in platformSegments)
            DrawSegment(segment, Color.gray, 0.05f);
    }

    private void DrawCurrentAndTargetSegments()
    {
        if (currentSegment != null)
            DrawSegment(currentSegment, Color.green, 0.08f);

        if (targetSegment != null)
            DrawSegment(targetSegment, Color.blue, 0.08f);
    }

    private void DrawPath()
    {
        if (pathSegments.Count == 0)
            return;

        Color pathColor = new Color(1f, 0.82f, 0f);
        for (int i = 0; i < pathSegments.Count; i++)
        {
            DrawSegment(pathSegments[i], pathColor, 0.07f);

            if (i >= pathSegments.Count - 1)
                continue;

            float referenceX = i == 0 && context != null && context.selfGroundPoint != null
                ? context.selfGroundPoint.position.x
                : GetSegmentCenterX(pathSegments[i]);

            GetConnectionPoints(pathSegments[i], pathSegments[i + 1], context, referenceX, out Vector2 from, out Vector2 to);
            Gizmos.color = pathColor;
            Gizmos.DrawLine(from, to);
            Gizmos.DrawWireSphere(from, 0.08f);
            Gizmos.DrawWireSphere(to, 0.08f);
        }

        if (hasNextSegment)
        {
            DrawSegment(nextSegment, pathColor, 0.12f);
            Gizmos.color = pathColor;
            Gizmos.DrawSphere(GetLandingPoint(currentSegment, nextSegment), 0.14f);
        }
    }

    private void DrawJumpTransitionDebug()
    {
        if (!hasJumpTransitionDebug)
            return;

        Color jumpColor = new Color(1f, 0.45f, 0f);
        Gizmos.color = jumpColor;
        Gizmos.DrawWireSphere(jumpTakeoffDebugPoint, 0.16f);
        Gizmos.DrawWireSphere(jumpLandingDebugPoint, 0.16f);
        Gizmos.DrawLine(jumpTakeoffDebugPoint, jumpLandingDebugPoint);
    }

    private void GetConnectionPoints(
        MapSegment.Segment fromSegment,
        MapSegment.Segment toSegment,
        MonsterContext context,
        float referenceX,
        out Vector2 from,
        out Vector2 to
    )
    {
        if (IsJumpTransition(fromSegment, toSegment)
            && TryGetJumpTransitionPoints(fromSegment, toSegment, context, referenceX, out from, out to))
            return;

        if (fromSegment.rightX >= toSegment.leftX && toSegment.rightX >= fromSegment.leftX)
        {
            float overlapLeft = Mathf.Max(fromSegment.leftX, toSegment.leftX);
            float overlapRight = Mathf.Min(fromSegment.rightX, toSegment.rightX);
            float x = (overlapLeft + overlapRight) * 0.5f;
            from = new Vector2(x, fromSegment.y);
            to = new Vector2(x, toSegment.y);
            return;
        }

        if (fromSegment.rightX < toSegment.leftX)
        {
            from = new Vector2(fromSegment.rightX, fromSegment.y);
            to = new Vector2(toSegment.leftX, toSegment.y);
            return;
        }

        from = new Vector2(fromSegment.leftX, fromSegment.y);
        to = new Vector2(toSegment.rightX, toSegment.y);
    }

    private bool IsJumpTransition(MapSegment.Segment fromSegment, MapSegment.Segment toSegment)
    {
        SegmentTransitionType type = GetSegmentTransitionType(fromSegment, toSegment);
        return IsJumpTransition(type);
    }

    private bool IsJumpTransition(SegmentTransitionType type)
    {
        return type == SegmentTransitionType.SameLevelGapJump
            || type == SegmentTransitionType.UpperJump
            || type == SegmentTransitionType.LowerJumpOrDrop;
    }

    private MonsterTraceMovePlan BuildJumpTransitionMovePlan(
        MonsterContext context,
        MapSegment.Segment fromSegment,
        MapSegment.Segment toSegment
    )
    {
        if (!TryGetJumpTransitionPoints(
                fromSegment,
                toSegment,
                context,
                context.selfGroundPoint.position.x,
                out Vector2 takeoffPoint,
                out Vector2 landingPoint))
        {
            return MonsterTraceMovePlan.Stop();
        }

        hasJumpTransitionDebug = true;
        jumpTakeoffDebugPoint = takeoffPoint;
        jumpLandingDebugPoint = landingPoint;

        Vector2 selfPoint = context.selfGroundPoint.position;
        float deltaToTakeoff = takeoffPoint.x - selfPoint.x;
        if (Mathf.Abs(deltaToTakeoff) > SameSegmentArriveTolerance)
            return MonsterTraceMovePlan.Move(deltaToTakeoff > 0f ? Vector3.right : Vector3.left);

        Vector3 landingDirection = GetJumpLandingDirection(selfPoint, landingPoint, fromSegment, toSegment);
        isFollowingJumpTransition = true;
        jumpAirMoveDirection = landingDirection;

        return MonsterTraceMovePlan.Move(landingDirection, jump: true);
    }

    private Vector3 GetJumpLandingDirection(
        Vector2 selfPoint,
        Vector2 landingPoint,
        MapSegment.Segment fromSegment,
        MapSegment.Segment toSegment
    )
    {
        float deltaToLanding = landingPoint.x - selfPoint.x;
        if (Mathf.Abs(deltaToLanding) > SameSegmentArriveTolerance)
            return deltaToLanding > 0f ? Vector3.right : Vector3.left;

        if (fromSegment.rightX < toSegment.leftX)
            return Vector3.right;

        if (fromSegment.leftX > toSegment.rightX)
            return Vector3.left;

        return Vector3.zero;
    }

    private SegmentTransition BuildSegmentTransition(
        MonsterContext context,
        MapSegment.Segment fromSegment,
        MapSegment.Segment toSegment
    )
    {
        SegmentTransitionType type = GetSegmentTransitionType(fromSegment, toSegment);
        return new SegmentTransition
        {
            type = type,
            moveDirection = type switch
            {
                SegmentTransitionType.SameLevelWalk => GetHorizontalTransitionDirection(context, fromSegment, toSegment),
                SegmentTransitionType.WalkDown => GetWalkDownDirection(context, fromSegment, toSegment),
                _ => Vector3.zero
            }
        };
    }

    private SegmentTransitionType GetSegmentTransitionType(MapSegment.Segment fromSegment, MapSegment.Segment toSegment)
    {
        float heightDelta = toSegment.y - fromSegment.y;

        if (Mathf.Abs(heightDelta) <= levelHeightTolerance)
        {
            return GetHorizontalGap(fromSegment, toSegment) <= WalkConnectionTolerance
                ? SegmentTransitionType.SameLevelWalk
                : SegmentTransitionType.SameLevelGapJump;
        }

        if (heightDelta > levelHeightTolerance)
            return SegmentTransitionType.UpperJump;

        return IsWalkDownTransition(fromSegment, toSegment)
            ? SegmentTransitionType.WalkDown
            : SegmentTransitionType.LowerJumpOrDrop;
    }

    private bool IsWalkDownTransition(MapSegment.Segment fromSegment, MapSegment.Segment toSegment)
    {
        if (GetHorizontalGap(fromSegment, toSegment) > WalkConnectionTolerance)
            return false;

        return IsWalkDownExitSupported(fromSegment.leftX, toSegment)
            || IsWalkDownExitSupported(fromSegment.rightX, toSegment)
            || SegmentsOverlapHorizontally(fromSegment, toSegment);
    }

    private bool IsWalkDownExitSupported(float exitX, MapSegment.Segment landingSegment)
    {
        return exitX >= landingSegment.leftX - WalkConnectionTolerance
            && exitX <= landingSegment.rightX + WalkConnectionTolerance;
    }

    private bool SegmentsOverlapHorizontally(MapSegment.Segment a, MapSegment.Segment b)
    {
        return a.rightX >= b.leftX && b.rightX >= a.leftX;
    }

    private Vector3 GetHorizontalTransitionDirection(
        MonsterContext context,
        MapSegment.Segment fromSegment,
        MapSegment.Segment toSegment
    )
    {
        if (fromSegment.rightX < toSegment.leftX)
            return Vector3.right;

        if (fromSegment.leftX > toSegment.rightX)
            return Vector3.left;

        return GetDirectionTowardRouteReference(context, fromSegment);
    }

    private Vector3 GetWalkDownDirection(
        MonsterContext context,
        MapSegment.Segment fromSegment,
        MapSegment.Segment toSegment
    )
    {
        bool canExitLeft = IsWalkDownExitSupported(fromSegment.leftX, toSegment);
        bool canExitRight = IsWalkDownExitSupported(fromSegment.rightX, toSegment);

        if (canExitLeft && !canExitRight)
            return Vector3.left;

        if (canExitRight && !canExitLeft)
            return Vector3.right;

        if (fromSegment.rightX < toSegment.leftX)
            return Vector3.right;

        if (fromSegment.leftX > toSegment.rightX)
            return Vector3.left;

        return GetDirectionTowardRouteReference(context, fromSegment);
    }

    private Vector3 GetDirectionTowardRouteReference(MonsterContext context, MapSegment.Segment current)
    {
        float selfX = context != null && context.selfGroundPoint != null
            ? context.selfGroundPoint.position.x
            : GetSegmentCenterX(current);

        float referenceX = context != null && context.target != null
            ? GetTargetGroundPoint(context).x
            : GetSegmentCenterX(current);

        float deltaX = referenceX - selfX;
        if (Mathf.Abs(deltaX) > SameSegmentArriveTolerance)
            return deltaX > 0f ? Vector3.right : Vector3.left;

        float leftDistance = Mathf.Abs(selfX - current.leftX);
        float rightDistance = Mathf.Abs(current.rightX - selfX);
        return leftDistance <= rightDistance ? Vector3.left : Vector3.right;
    }

    private bool TryGetJumpTransitionPoints(
        MapSegment.Segment fromSegment,
        MapSegment.Segment toSegment,
        MonsterContext context,
        float referenceX,
        out Vector2 from,
        out Vector2 to
    )
    {
        from = default;
        to = default;

        if (context == null || !TryCalculateJumpHorizontalDistance(context, fromSegment, toSegment, out float maxHorizontalDistance))
            return false;

        float takeoffLeft = Mathf.Max(fromSegment.leftX, toSegment.leftX - maxHorizontalDistance);
        float takeoffRight = Mathf.Min(fromSegment.rightX, toSegment.rightX + maxHorizontalDistance);
        if (takeoffLeft > takeoffRight)
            return false;

        float takeoffX = Mathf.Clamp(referenceX, takeoffLeft, takeoffRight);
        float landingX = Mathf.Clamp(takeoffX, toSegment.leftX, toSegment.rightX);

        from = new Vector2(takeoffX, fromSegment.y);
        to = new Vector2(landingX, toSegment.y);
        return true;
    }

    private float GetSegmentCenterX(MapSegment.Segment segment)
    {
        return (segment.leftX + segment.rightX) * 0.5f;
    }

    private void DrawSegment(MapSegment.Segment segment, Color color, float sphereRadius)
    {
        Gizmos.color = color;

        Vector3 left = new Vector3(segment.leftX, segment.y, 0f);
        Vector3 right = new Vector3(segment.rightX, segment.y, 0f);

        Gizmos.DrawLine(left, right);
        Gizmos.DrawSphere(left, sphereRadius);
        Gizmos.DrawSphere(right, sphereRadius);
    }

    private void DrawCandidatePoints()
    {
        foreach (LandingCandidate candidate in directReachCandidateDebugInfos)
        {
            Gizmos.color = GetCandidateColor(candidate.type);
            Gizmos.DrawWireSphere(candidate.position, 0.1f);
        }
    }

    private enum LandingCandidateType
    {
        SameLevel,
        Upper,
        Lower
    }

    private enum SegmentTransitionType
    {
        SameLevelWalk,
        WalkDown,
        SameLevelGapJump,
        UpperJump,
        LowerJumpOrDrop
    }

    private struct LandingCandidate
    {
        public Vector2 position;
        public LandingCandidateType type;
    }

    private struct SegmentTransition
    {
        public SegmentTransitionType type;
        public Vector3 moveDirection;
    }

    private struct TraceApproachGoal
    {
        public MapSegment.Segment segment;
        public Vector2 point;
    }

}
