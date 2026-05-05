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

    private const float SegmentTraversalBaseCost = 10f;
    private const float HeightChangeCostMultiplier = 1.5f;
    private const float DownwardDetourPenalty = 5f;
    private const float FallbackApproachTieTolerance = 0.5f;

    private MonsterContext context;
    private Rigidbody2D rigid;
    private MapSegment.Segment currentSegment;
    private MapSegment.Segment targetSegment;
    private MapSegment.Segment nextSegment;
    private bool hasNextSegment;

    public void Initialize(MonsterContext context)
    {
        this.context = context;
        rigid = GetComponent<Rigidbody2D>();
    }

    public MonsterTraceMovePlan CalculateMove(MonsterContext context)
    {
        MonsterContext activeContext = context ?? this.context;
        ClearDebugState();

        if (activeContext == null)
            return MonsterTraceMovePlan.Stop();

        if (activeContext.target == null || !activeContext.canMove)
            return MonsterTraceMovePlan.Stop();

        BuildLocalPlatformSegments(activeContext);
        BuildSegmentPath(activeContext);

        return MonsterTraceMovePlan.Stop();
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
        currentSegment = null;
        targetSegment = null;
        nextSegment = null;
        hasNextSegment = false;
    }

    private void BuildLocalPlatformSegments(MonsterContext context)
    {
        ResolveMapSegment();
        if (mapSegment == null || context.selfGroundPoint == null || context.target == null)
            return;

        Vector2 selfPoint = context.selfGroundPoint.position;
        Vector2 targetPoint = GetTargetGroundPoint(context);

        mapSegment.GetSegmentsInBounds(BuildSearchBounds(selfPoint, targetPoint), platformSegments);

        mapSegment.TryFindSegmentAtPoint(selfPoint, levelHeightTolerance, out currentSegment);
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

        if (currentSegment == targetSegment)
        {
            pathSegments.Add(currentSegment);
            return true;
        }

        Vector2 targetPoint = GetTargetGroundPoint(context);
        BuildReachabilityGraph(context, targetPoint);

        if (visitedSegments.Contains(targetSegment))
        {
            ReconstructPath(targetSegment);
            SelectNextSegment();
            return true;
        }

        MapSegment.Segment bestReachableSegment = FindBestReachableSegment(targetSegment);
        if (bestReachableSegment == null)
            return false;

        ReconstructPath(bestReachableSegment);
        SelectNextSegment();
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

    private MapSegment.Segment FindBestReachableSegment(MapSegment.Segment goalSegment)
    {
        MapSegment.Segment bestSegment = null;
        float bestApproachCost = float.MaxValue;
        float bestPathCost = float.MaxValue;

        foreach (MapSegment.Segment segment in visitedSegments)
        {
            if (segment == currentSegment)
                continue;

            float pathCost = segmentCosts.TryGetValue(segment, out float cost) ? cost : 0f;
            float approachCost = GetSegmentApproachCost(segment, goalSegment);
            if (IsBetterFallbackCandidate(approachCost, pathCost, bestApproachCost, bestPathCost))
            {
                bestApproachCost = approachCost;
                bestPathCost = pathCost;
                bestSegment = segment;
            }
        }

        return bestSegment;
    }

    private bool IsBetterFallbackCandidate(float approachCost, float pathCost, float bestApproachCost, float bestPathCost)
    {
        if (approachCost < bestApproachCost - FallbackApproachTieTolerance)
            return true;

        if (approachCost > bestApproachCost + FallbackApproachTieTolerance)
            return false;

        return pathCost < bestPathCost;
    }

    private float GetSegmentApproachCost(MapSegment.Segment from, MapSegment.Segment to)
    {
        return GetHorizontalGap(from, to) + (Mathf.Abs(to.y - from.y) * HeightChangeCostMultiplier);
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
        float maxHorizontalDistance = moveSpeed * airTime * jumpReachPadding;

        return GetHorizontalGap(from, to) <= maxHorizontalDistance;
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

    private float GetHorizontalGap(MapSegment.Segment from, MapSegment.Segment to)
    {
        if (from.rightX >= to.leftX && to.rightX >= from.leftX)
            return 0f;

        if (from.rightX < to.leftX)
            return to.leftX - from.rightX;

        return from.leftX - to.rightX;
    }

    private float GetDistanceFromSegmentToPoint(MapSegment.Segment segment, Vector2 point)
    {
        float x = Mathf.Clamp(point.x, segment.leftX, segment.rightX);
        return Vector2.Distance(new Vector2(x, segment.y), point);
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

            GetConnectionPoints(pathSegments[i], pathSegments[i + 1], out Vector2 from, out Vector2 to);
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

    private void GetConnectionPoints(MapSegment.Segment fromSegment, MapSegment.Segment toSegment, out Vector2 from, out Vector2 to)
    {
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

    private struct LandingCandidate
    {
        public Vector2 position;
        public LandingCandidateType type;
    }
}
