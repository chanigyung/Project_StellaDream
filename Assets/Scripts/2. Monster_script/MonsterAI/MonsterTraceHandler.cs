using UnityEngine;

public class MonsterTraceHandler : MonoBehaviour
{
    [Header("추적 대상과 범위")]
    [SerializeField] private LayerMask candidateLayers;
    [SerializeField] private float detectRadius = 6f;       // 탐지 시작 범위
    [SerializeField] private float loseRadius = 8f;         // 추적 유지(해제) 범위
    [SerializeField] private float pollInterval = 0.1f;     // 폴링 주기(초)

    [Header("추적 관련 변수")]
    [SerializeField] private float traceReleaseDelay = 1f;  // 이탈 후 추적 해제 대기
    [SerializeField] private float tracingSpeedMultiplier = 1.5f;
    [SerializeField] private float pendingSpeedMultiplier = 1f; // 해제 대기 중엔 원래 속도로 계속 따라감
    //TraceAction에서 읽어가는용
    public float TraceReleaseDelay => traceReleaseDelay;
    public float TracingSpeedMultiplier => tracingSpeedMultiplier;
    public float PendingSpeedMultiplier => pendingSpeedMultiplier;

    private GameObject desiredTarget;
    private bool damagedTriggered;

    public GameObject DesiredTarget => desiredTarget;
    public bool HasDamagedTrigger => damagedTriggered;

    [Header("Buffer")]
    [SerializeField] private int overlapBufferSize = 24;

    private MonsterContext context;

    // 폴링 타이머
    private float pollTimer = 0f;

    // Overlap 결과 버퍼(NonAlloc)
    private Collider2D[] overlapBuffer;

    public void Initialize(MonsterContext ctx)
    {
        context = ctx;

        // 버퍼 준비
        overlapBuffer = new Collider2D[Mathf.Max(4, overlapBufferSize)];

        // 시작하자마자 1회 검사하고 싶으면 0으로, 아니면 pollInterval로 둬도 됨
        pollTimer = 0f;
    }

    private void Update()
    {
        if (context == null) return;

        // 폴링 주기마다 감지/타겟 평가
        pollTimer -= Time.deltaTime;
        if (pollTimer <= 0f)
        {
            pollTimer = pollInterval;
            desiredTarget = SelectDesiredTarget();
        }
    }

    // 공격받으면 무조건 추적 + 영구 추적
    public void NotifyDamaged()
    {
        if (context == null) return;

        damagedTriggered = true;

        // 피격 시 즉시 1회 갱신해서 desiredTarget을 빠르게 확보(선택)
        desiredTarget = SelectDesiredTarget();
    }

    // 상태이상에서 on/off 할 수 있도록 공개 API
    public void SetRedirectTargetToNearestMonster(bool enabled)
    {
        if (context == null) return;
        context.attackMonster = enabled;
    }

    // -------------------------
    // 내부 로직
    // -------------------------

    private GameObject SelectDesiredTarget()
    {
        Vector2 center = context.selfTransform.position;

        // 추적 중이면 loseRadius로 유지, 아니면 detectRadius로 탐지
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

        monsterInstance = controller.Context.monsterInstance;
        return monsterInstance != null;
    }

    //traceAction에서 사용, 데미지 입을때 trace트리거용
    public bool DamagedTrigger()
    {
        if (!damagedTriggered) return false;
        damagedTriggered = false;
        return true;
    }

#if UNITY_EDITOR
    // 씬에서 감지 반경 확인용
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;
        if (context == null || context.selfTransform == null) return;

        Gizmos.DrawWireSphere(context.selfTransform.position, context.isTracing ? loseRadius : detectRadius);
    }
#endif
}
