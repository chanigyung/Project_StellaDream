using UnityEngine;

public class MonsterTraceHandler : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private LayerMask candidateLayers;     // [변경] Player + Monster 포함
    [SerializeField] private float detectRadius = 6f;       // 탐지 시작 범위
    [SerializeField] private float loseRadius = 8f;         // 추적 유지(해제) 범위
    [SerializeField] private float pollInterval = 0.1f;     // [추가] 폴링 주기(초)

    [Header("Trace")]
    [SerializeField] private float traceReleaseDelay = 1f;  // 이탈 후 추적 해제 대기(요구사항: 1초)
    [SerializeField] private float tracingSpeedMultiplier = 1.5f;
    [SerializeField] private float pendingSpeedMultiplier = 1f; // 요구사항: 해제 대기 중엔 원래 속도로 계속 따라감

    [Header("Buffer")]
    [SerializeField] private int overlapBufferSize = 24;

    private MonsterContext context;

    // [추가] 폴링 타이머
    private float pollTimer = 0f;

    // [추가] Overlap 결과 버퍼(NonAlloc)
    private Collider2D[] overlapBuffer;

    public void Initialize(MonsterContext ctx)
    {
        context = ctx;

        // [추가] 버퍼 준비
        overlapBuffer = new Collider2D[Mathf.Max(4, overlapBufferSize)];

        // [추가] 시작하자마자 1회 검사하고 싶으면 0으로, 아니면 pollInterval로 둬도 됨
        pollTimer = 0f;
    }

    private void Update()
    {
        if (context == null) return;

        // [추가] 폴링 주기마다 감지/타겟 평가
        pollTimer -= Time.deltaTime;
        if (pollTimer <= 0f)
        {
            pollTimer = pollInterval;
            EvaluateDetectionAndTarget();
        }

        // [기존 개념 유지] 추적 해제 대기 타이머 처리
        if (context.isTraceReleasedPending)
        {
            context.traceReleaseTimer -= Time.deltaTime;
            if (context.traceReleaseTimer <= 0f)
                EndTrace();
        }
    }

    // 공격받으면 무조건 추적 + 영구 추적
    public void NotifyDamaged()
    {
        if (!context.isTracing)
        {
            // [변경] 공격받을 때도 타겟을 잡아주기 위해 평가 1회
            EvaluateDetectionAndTarget();

            // 혹시 주변 후보가 없더라도 기본 플레이어 타겟은 유지
            if (context.target == null)
                context.target = GameObject.FindWithTag("Player");

            BeginTrace(context.target);
        }

        context.isTracePermanent = true;
        context.isTraceReleasedPending = false;
        context.traceReleaseTimer = 0f;
        context.instance.selfSpeedMultiplier = tracingSpeedMultiplier;
    }

    // [추가] 상태이상에서 on/off 할 수 있도록 공개 API
    public void SetRedirectTargetToNearestMonster(bool enabled)
    {
        if (context == null) return;
        context.attackMonster = enabled;
    }

    // -------------------------
    // 내부 로직
    // -------------------------

    private void EvaluateDetectionAndTarget()
    {
        GameObject desiredTarget = SelectDesiredTarget();

        if (desiredTarget != null)
        {
            if (!context.isTracing)
            {
                BeginTrace(desiredTarget);
                return;
            }

            // 추적 중이면 타겟 전환(상태이상에 의한 전환 포함)
            if (context.target != desiredTarget)
                context.target = desiredTarget;

            // 해제 대기 중이었다면 취소
            if (context.isTraceReleasedPending)
            {
                context.isTraceReleasedPending = false;
                context.traceReleaseTimer = 0f;
                context.instance.selfSpeedMultiplier = tracingSpeedMultiplier;
            }

            return;
        }

        // 후보가 없으면: 영구추적이 아니면 추적 해제 대기 시작
        if (context.isTracing && !context.isTracePermanent && !context.isTraceReleasedPending)
        {
            context.isTraceReleasedPending = true;
            context.traceReleaseTimer = traceReleaseDelay;

            // 요구사항: 해제 대기 중엔 원래 속도로 계속 따라감
            context.instance.selfSpeedMultiplier = pendingSpeedMultiplier;
        }
    }

    private GameObject SelectDesiredTarget()
    {
        Vector2 center = context.selfTransform.position;

        // [추가] 추적 중이면 loseRadius로 유지, 아니면 detectRadius로 탐지
        float radius = context.isTracing ? loseRadius : detectRadius;

        int count = Physics2D.OverlapCircleNonAlloc(center, radius, overlapBuffer, candidateLayers);
        if (count <= 0) return null;

        GameObject nearestPlayer = null;
        float nearestPlayerDistSqr = float.MaxValue;

        GameObject nearestMonster = null;
        float nearestMonsterDistSqr = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            Collider2D col = overlapBuffer[i];
            if (col == null) continue;

            float distSqr = ((Vector2)col.transform.position - center).sqrMagnitude;

            // Player 후보
            if (col.CompareTag("Player"))
            {
                if (distSqr < nearestPlayerDistSqr)
                {
                    nearestPlayerDistSqr = distSqr;
                    nearestPlayer = col.gameObject;
                }
                continue;
            }

            // Monster 후보: MonsterInstance 기준으로 자기 자신 제외
            if (TryGetMonsterInstance(col.gameObject, out MonsterInstance otherInstance))
            {
                if (otherInstance == null) continue;
                if (otherInstance == context.instance) continue; // [핵심] 자기 자신 제외

                if (distSqr < nearestMonsterDistSqr)
                {
                    nearestMonsterDistSqr = distSqr;
                    nearestMonster = col.gameObject;
                }
            }
        }

        // 합의한 룰:
        // 기본은 Player
        // 상태이상(redirect flag)일 때는 "몬스터가 있으면 몬스터", 없으면 Player
        if (context.attackMonster)
            return nearestMonster != null ? nearestMonster : nearestPlayer;

        return nearestPlayer;
    }

    private bool TryGetMonsterInstance(GameObject obj, out MonsterInstance monsterInstance)
    {
        monsterInstance = null;

        // 같은 오브젝트에 MonsterController가 없을 수도 있으니 parent까지 확인
        MonsterController controller = null;

        if (!obj.TryGetComponent(out controller))
            controller = obj.GetComponentInParent<MonsterController>();

        if (controller == null) return false;

        // MonsterController가 MonsterContext를 들고 있다고 가정(현재 업로드된 구조 기준)
        if (controller.Context == null) return false;

        monsterInstance = controller.Context.instance;
        return monsterInstance != null;
    }

    private void BeginTrace(GameObject targetObj)
    {
        context.isTracing = true;
        context.isTraceReleasedPending = false;
        context.traceReleaseTimer = 0f;

        context.target = targetObj != null ? targetObj : GameObject.FindWithTag("Player");
        context.instance.selfSpeedMultiplier = tracingSpeedMultiplier;
    }

    private void EndTrace()
    {
        context.isTracing = false;
        context.isTraceReleasedPending = false;
        context.traceReleaseTimer = 0f;

        context.instance.selfSpeedMultiplier = 1f;
        context.target = null;
        context.isTracePermanent = false; // [변경] 완전히 추적이 끝났으니 초기화
    }

#if UNITY_EDITOR
    // [추가] 씬에서 감지 반경 확인용
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;
        if (context == null || context.selfTransform == null) return;

        Gizmos.DrawWireSphere(context.selfTransform.position, context.isTracing ? loseRadius : detectRadius);
    }
#endif
}
