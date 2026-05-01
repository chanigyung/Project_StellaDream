using System.Collections.Generic;
using UnityEngine;

public class GroundTraceNavigator : MonoBehaviour, IMonsterTraceNavigator
{
    [Header("Legacy Jump")]
    [SerializeField]
    [Tooltip("Wall jump trigger height. Larger values make monsters jump only when the target is much higher.")]
    private float jumpTriggerHeight = 0.8f;

    [Header("Landing Candidate Search")]
    [SerializeField]
    [Tooltip("Layers treated as landable ground or platforms.")]
    private LayerMask landingLayer;

    [SerializeField]
    [Tooltip("Horizontal search range from the monster. Larger values find farther landing candidates.")]
    private float horizontalSearchDistance = 4f;

    [SerializeField]
    [Tooltip("Upward search range from the monster ground point. Larger values find higher platforms.")]
    private float verticalSearchUpDistance = 3f;

    [SerializeField]
    [Tooltip("Downward search range from the monster ground point. Larger values find lower landing points.")]
    private float verticalSearchDownDistance = 1.5f;

    [SerializeField]
    [Tooltip("Spacing between landing search samples. Smaller values are more accurate but more expensive.")]
    private float landingSampleSpacing = 0.35f;

    [SerializeField]
    [Tooltip("Height difference considered same-level. Larger values classify more candidates as same-level.")]
    private float levelHeightTolerance = 0.45f;

    [SerializeField]
    [Tooltip("Extra allowance for speed/jump reach checks. Larger values accept farther or higher candidates.")]
    private float jumpReachPadding = 1.15f;

    [Header("Debug")]
    [SerializeField]
    [Tooltip("Draws landing candidate search gizmos when this monster is selected.")]
    private bool drawDebugGizmos = true;

    [SerializeField]
    [Tooltip("Draws every landing search probe. Turning this off leaves only the selected candidate.")]
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

        if (!hasSelectedCandidate || selectedCandidate.type != LandingCandidateType.SameLevel)
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

        if (candidate.type == LandingCandidateType.Lower)
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
        else if (candidate.type == LandingCandidateType.SameLevel)
        {
            score += 10f;
        }

        return score;
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
            Gizmos.color = candidate.type == LandingCandidateType.Upper ? Color.cyan : Color.magenta;
            Gizmos.DrawWireSphere(candidate.position, 0.1f);
        }

        if (hasSelectedCandidate)
        {
            Gizmos.color = selectedCandidate.type == LandingCandidateType.Upper ? Color.cyan : Color.magenta;
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
