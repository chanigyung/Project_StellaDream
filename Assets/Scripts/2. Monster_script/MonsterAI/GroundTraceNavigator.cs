using System.Collections.Generic;
using UnityEngine;

public class GroundTraceNavigator : MonoBehaviour, IMonsterTraceNavigator
{
    [Header("Legacy Jump")]
    [Tooltip("벽 앞 점프를 시도할 목표와의 최소 높이 차이입니다. 값이 클수록 목표가 더 높을 때만 점프합니다.")]
    [SerializeField]
    private float jumpTriggerHeight = 0.8f;

    [Header("Landing Candidate Search")]
    [Tooltip("몬스터가 착지 가능한 지형/플랫폼으로 취급할 레이어입니다.")]
    [SerializeField]
    private LayerMask landingLayer;

    [Tooltip("몬스터 기준 좌우 착지 후보 탐색 거리입니다. 값이 클수록 더 먼 발판까지 찾습니다.")]
    [SerializeField]
    private float horizontalSearchDistance = 4f;

    [Tooltip("몬스터 발밑 기준 위쪽 착지 후보 탐색 거리입니다. 값이 클수록 더 높은 발판까지 찾습니다.")]
    [SerializeField]
    private float verticalSearchUpDistance = 3f;

    [Tooltip("몬스터 발밑 기준 아래쪽 착지 후보 탐색 거리입니다. 값이 클수록 더 낮은 발판까지 찾습니다.")]
    [SerializeField]
    private float verticalSearchDownDistance = 1.5f;

    [Tooltip("착지 후보를 검사하는 가로 샘플 간격입니다. 값이 작을수록 촘촘하지만 검사량이 늘어납니다.")]
    [SerializeField]
    private float landingSampleSpacing = 0.35f;

    [Tooltip("같은 높이로 간주할 높이 차이입니다. 값이 클수록 더 많은 발판을 같은 높이로 봅니다.")]
    [SerializeField]
    private float levelHeightTolerance = 0.45f;

    [Tooltip("낮은 발판 점프를 허용할 최대 하강 높이입니다. 값이 클수록 더 낮은 발판으로도 점프합니다.")]
    [SerializeField]
    private float lowerJumpHeightTolerance = 2f;

    [Tooltip("속도/점프력 기반 도달 판정 여유 배율입니다. 값이 클수록 더 멀거나 높은 후보를 허용합니다.")]
    [SerializeField]
    private float jumpReachPadding = 1.15f;

    [Header("Debug")]
    [Tooltip("몬스터 선택 시 착지 후보 탐색 Gizmo를 표시합니다.")]
    [SerializeField]
    private bool drawDebugGizmos = true;

    [Tooltip("모든 착지 후보 검사선을 표시합니다. 끄면 선택 후보 중심으로만 확인하기 쉽습니다.")]
    [SerializeField]
    private bool drawProbeGizmos = true;

    private readonly List<LandingProbeDebugInfo> probeDebugInfos = new();
    private readonly List<LandingCandidate> acceptedCandidateDebugInfos = new();
    private MonsterContext context;
    private LandingCandidate selectedCandidate;
    private bool hasSelectedCandidate;

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

        TryFindBestLandingCandidate(activeContext, out selectedCandidate);

        float dirX = activeContext.directionToTarget.x;
        if (Mathf.Abs(dirX) < 0.01f)
            dirX = activeContext.facingDirectionX;

        Vector3 moveDirection = (dirX < 0f) ? Vector3.left : Vector3.right;
        bool shouldJump = ShouldJumpForHigherTarget(activeContext);

        if (activeContext.isGrounded)
        {
            bool hasGround = (moveDirection == Vector3.left)
                ? activeContext.hasGroundLeft
                : activeContext.hasGroundRight;

            if (!hasGround)
            {
                if (TryBuildSameLevelGapJumpPlan(activeContext, moveDirection, out MonsterTraceMovePlan gapJumpPlan))
                    return gapJumpPlan;

                return MonsterTraceMovePlan.Stop();
            }
        }

        return MonsterTraceMovePlan.Move(moveDirection, shouldJump);
    }

    private void ClearDebugState()
    {
        probeDebugInfos.Clear();
        acceptedCandidateDebugInfos.Clear();
        hasSelectedCandidate = false;
        selectedCandidate = default;
    }

    private bool ShouldJumpForHigherTarget(MonsterContext context)
    {
        if (!context.hasWallAhead || context.selfGroundPoint == null || context.target == null)
            return false;

        UnitController targetUnit = context.target.GetComponent<UnitController>();
        if (targetUnit == null)
            return false;

        float deltaY = targetUnit.GroundPoint.position.y - context.selfGroundPoint.position.y;
        return deltaY > jumpTriggerHeight;
    }

    private bool TryBuildSameLevelGapJumpPlan(MonsterContext context, Vector3 defaultMoveDirection, out MonsterTraceMovePlan plan)
    {
        plan = default;

        if (!hasSelectedCandidate || !CanUseForGapJump(selectedCandidate))
            return false;

        float candidateDirX = selectedCandidate.position.x - context.selfGroundPoint.position.x;
        if (Mathf.Abs(candidateDirX) < 0.01f)
            return false;

        if (Mathf.Sign(candidateDirX) != Mathf.Sign(defaultMoveDirection.x))
            return false;

        Vector3 jumpMoveDirection = candidateDirX < 0f ? Vector3.left : Vector3.right;
        plan = MonsterTraceMovePlan.Move(jumpMoveDirection, true);
        return true;
    }

    private bool CanUseForGapJump(LandingCandidate candidate)
    {
        if (candidate.type == LandingCandidateType.SameLevel)
            return true;

        if (candidate.type != LandingCandidateType.Lower)
            return false;

        return Mathf.Abs(candidate.heightDelta) <= lowerJumpHeightTolerance;
    }

    private bool TryFindBestLandingCandidate(MonsterContext context, out LandingCandidate candidate)
    {
        candidate = default;

        if (context.selfGroundPoint == null || context.target == null || landingLayer.value == 0)
            return false;

        Vector2 originPoint = context.selfGroundPoint.position;
        Vector2 targetPoint = GetTargetGroundPoint(context);
        float targetHeightDelta = targetPoint.y - originPoint.y;

        bool found = false;
        LandingCandidate bestCandidate = default;
        float bestScore = float.NegativeInfinity;

        float spacing = Mathf.Max(0.05f, landingSampleSpacing);
        int sampleCount = Mathf.CeilToInt(horizontalSearchDistance / spacing);

        for (int side = -1; side <= 1; side += 2)
        {
            for (int i = 1; i <= sampleCount; i++)
            {
                float offsetX = Mathf.Min(i * spacing, horizontalSearchDistance) * side;
                float sampleX = originPoint.x + offsetX;

                if (!TryProbeLandingAtX(sampleX, originPoint.y, out Vector2 landingPoint))
                    continue;

                if (IsCurrentGroundPoint(originPoint, landingPoint))
                    continue;

                LandingCandidate current = BuildLandingCandidate(originPoint, landingPoint);
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

    private bool TryProbeLandingAtX(float sampleX, float referenceY, out Vector2 landingPoint)
    {
        float topY = referenceY + Mathf.Max(0f, verticalSearchUpDistance);
        float distance = Mathf.Max(0.05f, verticalSearchUpDistance + verticalSearchDownDistance);
        Vector2 origin = new Vector2(sampleX, topY);
        Vector2 end = origin + Vector2.down * distance;

        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, distance, landingLayer);
        if (hit.collider == null)
        {
            landingPoint = default;
            probeDebugInfos.Add(new LandingProbeDebugInfo(origin, end, false, default, false));
            return false;
        }

        landingPoint = hit.point;
        probeDebugInfos.Add(new LandingProbeDebugInfo(origin, end, true, landingPoint, false));
        return true;
    }

    private bool IsCurrentGroundPoint(Vector2 originPoint, Vector2 landingPoint)
    {
        return Mathf.Abs(landingPoint.y - originPoint.y) <= levelHeightTolerance
            && Mathf.Abs(landingPoint.x - originPoint.x) <= landingSampleSpacing * 1.5f;
    }

    private LandingCandidate BuildLandingCandidate(Vector2 originPoint, Vector2 landingPoint)
    {
        float heightDelta = landingPoint.y - originPoint.y;
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

        if (context != null && context.selfGroundPoint != null)
        {
            Vector2 center = context.selfGroundPoint.position;
            Vector2 size = new Vector2(horizontalSearchDistance * 2f, verticalSearchUpDistance + verticalSearchDownDistance);
            Vector2 boxCenter = center + Vector2.up * ((verticalSearchUpDistance - verticalSearchDownDistance) * 0.5f);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(boxCenter, size);
        }

        if (drawProbeGizmos)
        {
            foreach (LandingProbeDebugInfo info in probeDebugInfos)
            {
                Gizmos.color = info.hit ? Color.green : Color.red;
                Gizmos.DrawLine(info.origin, info.end);

                if (info.hit)
                    Gizmos.DrawSphere(info.hitPoint, 0.04f);
                else
                    Gizmos.DrawWireSphere(info.end, 0.04f);
            }
        }

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

    private readonly struct LandingProbeDebugInfo
    {
        public readonly Vector2 origin;
        public readonly Vector2 end;
        public readonly bool hit;
        public readonly Vector2 hitPoint;
        public readonly bool rejected;

        public LandingProbeDebugInfo(Vector2 origin, Vector2 end, bool hit, Vector2 hitPoint, bool rejected)
        {
            this.origin = origin;
            this.end = end;
            this.hit = hit;
            this.hitPoint = hitPoint;
            this.rejected = rejected;
        }
    }
}
