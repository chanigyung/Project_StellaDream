using System.Collections.Generic;
using UnityEngine;

public class GroundTraceNavigator : MonoBehaviour, IMonsterTraceNavigator
{
    [Header("Map Segment")]
    [Tooltip("씬의 발판 구간 캐시입니다. 비워두면 씬의 MapSegment를 자동으로 찾습니다.")]
    [SerializeField] private MapSegment mapSegment;

    [Header("Local Query")]
    [Tooltip("몬스터/타겟 기준 좌우 발판 조회 거리입니다. 값이 클수록 더 넓은 지역의 발판 구간을 조회합니다.")]
    [SerializeField] private float horizontalSearchDistance = 4f;

    [Tooltip("기준 발밑 위치에서 위쪽으로 발판을 조회하는 거리입니다. 값이 클수록 더 높은 플랫폼까지 조회합니다.")]
    [SerializeField] private float verticalSearchUpDistance = 3f;

    [Tooltip("기준 발밑 위치에서 아래쪽으로 발판을 조회하는 거리입니다. 값이 클수록 더 낮은 플랫폼까지 조회합니다.")]
    [SerializeField] private float verticalSearchDownDistance = 3f;

    [Tooltip("같은 발판 구간으로 판단할 높이 차이입니다. 값이 클수록 약간 다른 높이도 같은 구간으로 봅니다.")]
    [SerializeField] private float levelHeightTolerance = 0.45f;

    [Tooltip("낮은 발판 점프 후보를 허용할 최대 하강 높이입니다. 값이 클수록 더 낮은 발판도 후보가 됩니다.")]
    [SerializeField] private float lowerJumpHeightTolerance = 2f;

    [Tooltip("속도/점프력 기반 도달 판정 여유 배율입니다. 값이 클수록 더 멀거나 높은 후보를 허용합니다.")]
    [SerializeField] private float jumpReachPadding = 1.15f;

    [Header("Debug")]
    [Tooltip("몬스터 선택 시 지형 탐색 Gizmo를 표시합니다.")]
    [SerializeField] private bool drawDebugGizmos = true;

    [Tooltip("몬스터가 경로 탐색에 사용하는 통합 조회 범위 박스를 표시합니다.")]
    [SerializeField] private bool drawSearchBoundsGizmos = true;

    [Tooltip("점 단위 착지 후보를 표시합니다. 끄면 발판 구간만 보기 쉽습니다.")]
    [SerializeField] private bool drawCandidateGizmos = true;

    [Tooltip("조회된 발판 구간을 표시합니다. current는 초록, target은 파란색으로 표시됩니다.")]
    [SerializeField] private bool drawSegmentGizmos = true;

    private readonly List<LandingCandidate> acceptedCandidateDebugInfos = new();
    private readonly List<MapSegment.Segment> platformSegments = new();

    private MonsterContext context;
    private LandingCandidate selectedCandidate;
    private bool hasSelectedCandidate;
    private MapSegment.Segment currentSegment;
    private MapSegment.Segment targetSegment;

    public void Initialize(MonsterContext context)
    {
        this.context = context;
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
        TryFindBestLandingCandidate(activeContext, out selectedCandidate);

        return MonsterTraceMovePlan.Stop();
    }

    private void ClearDebugState()
    {
        acceptedCandidateDebugInfos.Clear();
        platformSegments.Clear();
        hasSelectedCandidate = false;
        selectedCandidate = default;
        currentSegment = null;
        targetSegment = null;
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
        mapSegment.TryFindSegmentAtPoint(targetPoint, levelHeightTolerance, out targetSegment);
    }

    private void ResolveMapSegment()
    {
        if (mapSegment != null)
            return;

        mapSegment = MapSegment.Instance != null ? MapSegment.Instance : FindObjectOfType<MapSegment>();
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

    private bool TryFindBestLandingCandidate(MonsterContext context, out LandingCandidate candidate)
    {
        candidate = default;

        if (context.selfGroundPoint == null || context.target == null)
            return false;

        Vector2 originPoint = context.selfGroundPoint.position;
        Vector2 targetPoint = GetTargetGroundPoint(context);
        float targetHeightDelta = targetPoint.y - originPoint.y;

        bool found = false;
        LandingCandidate bestCandidate = default;
        float bestScore = float.NegativeInfinity;

        foreach (MapSegment.Segment segment in platformSegments)
        {
            if (segment == currentSegment)
                continue;

            LandingCandidate current = BuildLandingCandidate(originPoint, segment);
            if (!CanReachCandidate(context, current))
                continue;

            acceptedCandidateDebugInfos.Add(current);

            float score = ScoreCandidate(current, targetPoint, targetHeightDelta);
            if (!found || score > bestScore)
            {
                found = true;
                bestScore = score;
                bestCandidate = current;
            }
        }

        if (!found)
            return false;

        candidate = bestCandidate;
        selectedCandidate = candidate;
        hasSelectedCandidate = true;
        return true;
    }

    private Vector2 GetTargetGroundPoint(MonsterContext context)
    {
        UnitController targetUnit = context.target.GetComponent<UnitController>();
        if (targetUnit != null)
            return targetUnit.GroundPoint.position;

        return context.target.transform.position;
    }

    private LandingCandidate BuildLandingCandidate(Vector2 originPoint, MapSegment.Segment segment)
    {
        Vector2 landingPoint = new Vector2(Mathf.Clamp(originPoint.x, segment.leftX, segment.rightX), segment.y);
        float heightDelta = segment.y - originPoint.y;
        LandingCandidateType type = Mathf.Abs(heightDelta) <= levelHeightTolerance
            ? LandingCandidateType.SameLevel
            : heightDelta > 0f
                ? LandingCandidateType.Upper
                : LandingCandidateType.Lower;

        return new LandingCandidate
        {
            position = landingPoint,
            type = type,
            horizontalDistance = Mathf.Abs(landingPoint.x - originPoint.x),
            heightDelta = heightDelta
        };
    }

    private bool CanReachCandidate(MonsterContext context, LandingCandidate candidate)
    {
        if (context.instance == null)
            return false;

        if (candidate.type == LandingCandidateType.Lower && Mathf.Abs(candidate.heightDelta) > lowerJumpHeightTolerance)
            return false;

        float jumpPower = context.instance.GetCurrentJumpPower();
        float moveSpeed = context.instance.GetCurrentMoveSpeed();
        float gravity = Mathf.Abs(Physics2D.gravity.y);

        if (jumpPower <= 0f || moveSpeed <= 0f || gravity <= 0f)
            return false;

        float maxJumpHeight = (jumpPower * jumpPower) / (2f * gravity);
        if (candidate.heightDelta > maxJumpHeight * jumpReachPadding)
            return false;

        float fallHeight = Mathf.Max(0f, maxJumpHeight - candidate.heightDelta);
        float timeUp = jumpPower / gravity;
        float timeDown = Mathf.Sqrt((2f * fallHeight) / gravity);
        float airTime = timeUp + timeDown;
        float maxHorizontalDistance = moveSpeed * airTime * jumpReachPadding;

        return candidate.horizontalDistance <= maxHorizontalDistance;
    }

    private float ScoreCandidate(LandingCandidate candidate, Vector2 targetPoint, float targetHeightDelta)
    {
        float distanceToTarget = Vector2.Distance(candidate.position, targetPoint);
        float score = -distanceToTarget;

        if (targetHeightDelta > levelHeightTolerance && candidate.type == LandingCandidateType.Upper)
        {
            bool isIntermediateHeight = candidate.heightDelta < targetHeightDelta - levelHeightTolerance;
            score += isIntermediateHeight ? 200f + candidate.heightDelta : 50f;
        }
        else if (targetHeightDelta < -levelHeightTolerance && candidate.type == LandingCandidateType.Lower)
        {
            float heightMatch = -Mathf.Abs(candidate.heightDelta - targetHeightDelta);
            score += 200f + heightMatch;
        }
        else if (candidate.type == LandingCandidateType.SameLevel)
        {
            score += 10f;
        }
        else if (candidate.type == LandingCandidateType.Lower)
        {
            score += 5f;
        }

        return score;
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
            DrawPlatformSegments();

        if (drawCandidateGizmos)
            DrawCandidatePoints();
    }

    private void DrawSearchBounds()
    {
        if (context == null || context.selfGroundPoint == null)
            return;

        if (context.target != null)
            DrawSearchBoundsAt(context.selfGroundPoint.position, GetTargetGroundPoint(context), Color.yellow);
    }

    private void DrawSearchBoundsAt(Vector2 selfPoint, Vector2 targetPoint, Color color)
    {
        Bounds bounds = BuildSearchBounds(selfPoint, targetPoint);
        Gizmos.color = color;
        Gizmos.DrawWireCube(bounds.center, bounds.size);
    }

    private void DrawPlatformSegments()
    {
        foreach (MapSegment.Segment segment in platformSegments)
            DrawSegment(segment, Color.gray, 0.05f);

        if (currentSegment != null)
            DrawSegment(currentSegment, Color.green, 0.08f);

        if (targetSegment != null)
            DrawSegment(targetSegment, Color.blue, 0.08f);
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
        foreach (LandingCandidate candidate in acceptedCandidateDebugInfos)
        {
            Gizmos.color = GetCandidateColor(candidate.type);
            Gizmos.DrawWireSphere(candidate.position, 0.1f);
        }

        if (hasSelectedCandidate)
        {
            Gizmos.color = GetCandidateColor(selectedCandidate.type);
            Gizmos.DrawSphere(selectedCandidate.position, 0.12f);
            Gizmos.DrawWireSphere(selectedCandidate.position, 0.2f);
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
        public float horizontalDistance;
        public float heightDelta;
    }
}
