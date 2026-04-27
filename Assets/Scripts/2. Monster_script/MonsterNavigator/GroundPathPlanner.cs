using System.Collections.Generic;
using UnityEngine;

// Planner that validates and follows walk-only paths on the same ground surface.
public class GroundPathPlanner : MonsterPathPlanner
{
    [Header("Walk Path")]
    [Tooltip("경로 샘플링 시 걸을 수 있는 바닥/플랫폼으로 취급할 레이어입니다.")]
    [SerializeField] private LayerMask groundLayer;
    [Tooltip("걷기 경로를 검사할 때 샘플 사이의 가로 간격입니다. 작을수록 정밀하지만 검사 횟수가 늘어납니다.")]
    [SerializeField] private float sampleSpacing = 0.35f;
    [Tooltip("바닥 탐색 Raycast를 시작할 기준 지점 위쪽 높이입니다.")]
    [SerializeField] private float groundProbeHeight = 0.5f;
    [Tooltip("각 샘플 지점에서 아래쪽 바닥을 찾기 위해 추가로 검사할 거리입니다.")]
    [SerializeField] private float groundProbeDistance = 3f;
    [Tooltip("인접한 걷기 샘플 사이에서 걸어서 이동 가능하다고 볼 최대 높이 차이입니다.")]
    [SerializeField] private float maxStepHeight = 0.35f;
    [Tooltip("목적지와 이 가로 거리 이하로 가까워지면 도착한 것으로 판단합니다.")]
    [SerializeField] private float waypointReachDistance = 0.2f;
    [Tooltip("한 번의 경로 요청에서 걷기/낙하 경로를 검사할 최대 가로 거리입니다.")]
    [SerializeField] private float maxPlanDistance = 12f;

    [Header("Drop Path")]
    [Tooltip("아래쪽에 유효한 착지 지점이 있으면 추적 중 의도적으로 낭떠러지에서 떨어질 수 있게 합니다.")]
    [SerializeField] private bool allowDropPath = true;
    [Tooltip("직선 지상 경로가 막혔을 때 낭떠러지 시작점을 찾기 위해 앞쪽으로 검사할 거리입니다.")]
    [SerializeField] private float ledgeSearchDistance = 1.2f;
    [Tooltip("낭떠러지 너머 아래 착지 지점을 찾기 위해 앞쪽으로 더 밀어 검사하는 거리입니다.")]
    [SerializeField] private float dropForwardOffset = 0.25f;
    [Tooltip("막힌 경로를 낙하 경로로 인정하기 위한 최소 낙하 높이입니다.")]
    [SerializeField] private float minDropHeight = 0.45f;
    [Tooltip("의도적으로 떨어질 수 있는 최대 낙하 높이입니다.")]
    [SerializeField] private float maxDropHeight = 4f;
    [Tooltip("감지된 착지 지점과 목표 지점 사이에 허용할 세로 높이 차이입니다.")]
    [SerializeField] private float dropDestinationHeightTolerance = 1f;

    [Header("Debug")]
    [Tooltip("몬스터를 선택했을 때 현재 계산된 걷기/낙하 경로를 Gizmo로 표시합니다.")]
    [SerializeField] private bool drawDebugGizmos = true;
    [Tooltip("몬스터를 선택했을 때 경로 탐색에 사용한 아래 방향 Raycast를 Gizmo로 표시합니다.")]
    [SerializeField] private bool drawProbeGizmos = true;

    private readonly List<MonsterPathWaypoint> currentPath = new();
    private readonly List<ProbeDebugInfo> probeDebugInfos = new();

    public override bool CanPlan(MonsterPathRequest request)
    {
        return context != null
            && request.moveType == MonsterMoveType.Ground
            && groundLayer.value != 0;
    }

    public override bool TryFindPath(MonsterPathRequest request, out MonsterPathResult result)
    {
        result = MonsterPathResult.Failed();

        if (!CanPlan(request))
            return false;

        Vector2 start = GetGroundPoint(request.start);
        Vector2 destination = request.destination;
        float deltaX = destination.x - start.x;
        probeDebugInfos.Clear();

        if (Mathf.Abs(deltaX) <= waypointReachDistance)
        {
            result = MonsterPathResult.FromCommand(MonsterMoveCommand.Stop(MonsterMoveType.Ground));
            currentPath.Clear();
            return true;
        }

        if (Mathf.Abs(deltaX) > maxPlanDistance)
            return false;

        float directionX = Mathf.Sign(deltaX);
        if (!TryBuildWalkPath(start, destination.x, directionX))
        {
            if (!TryBuildDropPath(request, start, destination, directionX))
                return false;

            MonsterMoveCommand dropCommand = MonsterMoveCommand.Ground(directionX, false, true);
            result = MonsterPathResult.FromCommand(dropCommand, new List<MonsterPathWaypoint>(currentPath));
            return true;
        }

        MonsterMoveCommand command = MonsterMoveCommand.Ground(directionX);
        result = MonsterPathResult.FromCommand(command, new List<MonsterPathWaypoint>(currentPath));
        return true;
    }

    private Vector2 GetGroundPoint(Vector2 fallback)
    {
        if (context != null && context.selfGroundPoint != null)
            return context.selfGroundPoint.position;

        return fallback;
    }

    private bool TryBuildWalkPath(Vector2 start, float destinationX, float directionX)
    {
        currentPath.Clear();
        probeDebugInfos.Clear();

        if (!TryFindGroundAtX(start.x, start.y, out Vector2 previousGround))
            return false;

        float distanceX = Mathf.Abs(destinationX - start.x);
        int sampleCount = Mathf.CeilToInt(distanceX / Mathf.Max(0.05f, sampleSpacing));

        for (int i = 1; i <= sampleCount; i++)
        {
            float sampleX = i == sampleCount
                ? destinationX
                : start.x + directionX * sampleSpacing * i;

            if (!TryFindGroundAtX(sampleX, previousGround.y, out Vector2 sampleGround))
            {
                currentPath.Clear();
                return false;
            }

            if (Mathf.Abs(sampleGround.y - previousGround.y) > maxStepHeight)
            {
                currentPath.Clear();
                return false;
            }

            currentPath.Add(new MonsterPathWaypoint(sampleGround, MonsterPathEdgeType.Walk));
            previousGround = sampleGround;
        }

        return currentPath.Count > 0;
    }

    private bool TryBuildDropPath(MonsterPathRequest request, Vector2 start, Vector2 destination, float directionX)
    {
        if (!allowDropPath || !request.directPathBlocked)
            return false;

        if (destination.y > start.y - minDropHeight)
            return false;

        currentPath.Clear();

        if (!TryFindGroundAtX(start.x, start.y, out Vector2 previousGround))
            return false;

        float maxSearchX = start.x + directionX * ledgeSearchDistance;
        float searchDistanceX = Mathf.Abs(maxSearchX - start.x);
        int sampleCount = Mathf.CeilToInt(searchDistanceX / Mathf.Max(0.05f, sampleSpacing));

        for (int i = 1; i <= sampleCount; i++)
        {
            float sampleX = start.x + directionX * sampleSpacing * i;
            if (directionX > 0f)
                sampleX = Mathf.Min(sampleX, maxSearchX);
            else
                sampleX = Mathf.Max(sampleX, maxSearchX);

            if (TryFindGroundAtX(sampleX, previousGround.y, out Vector2 sampleGround))
            {
                if (Mathf.Abs(sampleGround.y - previousGround.y) > maxStepHeight)
                    return false;

                currentPath.Add(new MonsterPathWaypoint(sampleGround, MonsterPathEdgeType.Walk));
                previousGround = sampleGround;
                continue;
            }

            float landingX = sampleX + directionX * dropForwardOffset;
            if (!TryFindDropLanding(landingX, previousGround.y, out Vector2 landingPoint))
                return false;

            if (previousGround.y - landingPoint.y < minDropHeight)
                return false;

            if (Mathf.Abs(destination.y - landingPoint.y) > dropDestinationHeightTolerance)
                return false;

            currentPath.Add(new MonsterPathWaypoint(landingPoint, MonsterPathEdgeType.Drop));
            return true;
        }

        currentPath.Clear();
        return false;
    }

    private bool TryFindDropLanding(float x, float ledgeY, out Vector2 landingPoint)
    {
        Vector2 origin = new Vector2(x, ledgeY - minDropHeight + groundProbeHeight);
        float distance = groundProbeHeight + Mathf.Max(0f, maxDropHeight - minDropHeight);
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, distance, groundLayer);
        Vector2 end = origin + Vector2.down * distance;

        if (hit.collider == null)
        {
            landingPoint = default;
            probeDebugInfos.Add(new ProbeDebugInfo(origin, end, false, default));
            return false;
        }

        landingPoint = hit.point;
        probeDebugInfos.Add(new ProbeDebugInfo(origin, end, true, landingPoint));
        return true;
    }

    private bool TryFindGroundAtX(float x, float referenceY, out Vector2 groundPoint)
    {
        Vector2 origin = new Vector2(x, referenceY + groundProbeHeight);
        float distance = groundProbeHeight + groundProbeDistance;
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, distance, groundLayer);
        Vector2 end = origin + Vector2.down * distance;

        if (hit.collider == null)
        {
            groundPoint = default;
            probeDebugInfos.Add(new ProbeDebugInfo(origin, end, false, default));
            return false;
        }

        groundPoint = hit.point;
        probeDebugInfos.Add(new ProbeDebugInfo(origin, end, true, groundPoint));
        return true;
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawDebugGizmos)
            return;

        if (drawProbeGizmos && probeDebugInfos != null)
        {
            foreach (ProbeDebugInfo info in probeDebugInfos)
            {
                Gizmos.color = info.hit ? Color.green : Color.red;
                Gizmos.DrawLine(info.origin, info.end);

                if (info.hit)
                    Gizmos.DrawSphere(info.hitPoint, 0.05f);
                else
                    Gizmos.DrawWireSphere(info.end, 0.05f);
            }
        }

        if (currentPath == null || currentPath.Count == 0)
            return;

        Gizmos.color = Color.cyan;
        for (int i = 0; i < currentPath.Count; i++)
        {
            Vector3 point = currentPath[i].position;
            Gizmos.DrawSphere(point, 0.06f);

            if (i > 0)
                Gizmos.DrawLine(currentPath[i - 1].position, point);
        }
    }

    private readonly struct ProbeDebugInfo
    {
        public readonly Vector2 origin;
        public readonly Vector2 end;
        public readonly bool hit;
        public readonly Vector2 hitPoint;

        public ProbeDebugInfo(Vector2 origin, Vector2 end, bool hit, Vector2 hitPoint)
        {
            this.origin = origin;
            this.end = end;
            this.hit = hit;
            this.hitPoint = hitPoint;
        }
    }
}
