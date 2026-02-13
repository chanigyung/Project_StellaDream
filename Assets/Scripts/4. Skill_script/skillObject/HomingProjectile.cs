using System.Collections.Generic;
using UnityEngine;

public class HomingProjectile : Projectile
{
    [Header("최초 발사시 거리 및 범위")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float initialStraightTime = 0.3f;
    [SerializeField] private float initialSearchRadius = 2.5f;

    [Header("투사체 히트 판정 거리")]
    [SerializeField] private float hitRadius = 0.01f;

    [Header("타격 가능 횟수")]
    [SerializeField] private int maxHitCount = 5;

    [Header("복귀 전 이동거리")]
    [SerializeField] private float returnStopDistance = 0.3f;

    // 타격 후보군 - 타격 여부 기록용 HashSet
    private readonly HashSet<GameObject> hitTargetSet = new();

    private Transform caster;
    private Transform target;

    private float elapsed;
    private bool hasFirstHit;
    private bool returning;

    // 직전에 히트한 대상(연속 히트 방지용, 루트 기준)
    private GameObject lastHitTarget;

    // 최초 직진 중 탐색 타이머
    private float initialRetargetTimer;
    private const float InitialRetargetInterval = 0.05f;

    // 카메라 범위 탐색 타이머
    private float cameraRetargetTimer;
    private const float CameraRetargetInterval = 0.08f;

    /// 투사체 초기화 (Homing은 lifetime을 사용하지 않음)
    public override void Initialize(GameObject attacker, SkillInstance skill, Vector2 direction, float speed, float lifetime)
    {
        base.Initialize(attacker, skill, direction, speed, 0f);

        caster = attacker != null ? attacker.transform : null;

        elapsed = 0f;
        hasFirstHit = false;
        returning = false;
        lastHitTarget = null;
        target = null;

        hitTargetSet.Clear();

        initialRetargetTimer = 0f;
        cameraRetargetTimer = 0f;
    }

    /// 투사체 전체 이동 루프 (직진 → 추적 → 귀환)
    protected override void Move()
    {
        if (!initialized) return;

        if (caster == null)
        {
            Destroy(gameObject);
            return;
        }

        elapsed += Time.deltaTime;

        if (returning)
        {
            MoveReturn();
            return;
        }

        // 1) 최초 직진 구간
        if (elapsed < initialStraightTime)
        {
            transform.position += transform.right * (speed * Time.deltaTime);

            initialRetargetTimer += Time.deltaTime;
            if (initialRetargetTimer >= InitialRetargetInterval && target == null)
            {
                initialRetargetTimer = 0f;
                TryAcquireInitialTarget();
            }

            // 변경: 직진 중이라도 가까운 적을 찾으면 즉시 추적 단계로 넘어감
            if (target == null)
                return;
        }

        // 2) 타겟 탐색
        if (target == null)
        {
            if (!hasFirstHit)
            {
                TryAcquireInitialTarget();
            }
            else
            {
                cameraRetargetTimer += Time.deltaTime;
                if (cameraRetargetTimer >= CameraRetargetInterval)
                {
                    cameraRetargetTimer = 0f;
                    TryAcquireCameraTarget();
                }
            }
        }

        // 3) 타겟 없으면 귀환
        if (target == null)
        {
            BeginReturn();
            return;
        }

        // 4) 타겟 중심 도달 기반 히트 판정
        Vector2 targetCenter = GetTargetCenter(target);
        Vector2 toTarget = targetCenter - (Vector2)transform.position;

        if (toTarget.sqrMagnitude <= hitRadius * hitRadius)
        {
            HandleHit(target.gameObject);
            return;
        }

        // 5) 직선 이동
        Vector2 dir = toTarget.normalized;
        transform.right = dir;
        transform.position += (Vector3)dir * (speed * Time.deltaTime);
    }

    /// 플레이어에게 귀환하는 이동 처리
    private void MoveReturn()
    {
        Vector2 toCaster = (Vector2)caster.position - (Vector2)transform.position;

        if (toCaster.sqrMagnitude <= returnStopDistance * returnStopDistance)
        {
            Destroy(gameObject);
            return;
        }

        Vector2 dir = toCaster.normalized;
        transform.right = dir;
        transform.position += (Vector3)dir * (speed * Time.deltaTime);
    }

    /// 최초 직진 이후, 근거리 범위에서 다음 타겟 탐색
    private void TryAcquireInitialTarget()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, initialSearchRadius, enemyLayer);

        float bestDist = float.MaxValue;
        Transform best = null;
        HashSet<GameObject> uniqueTargets = new();

        foreach (var hit in hits)
        {
            var damageable = hit.GetComponentInParent<IDamageable>() as Component;
            if (damageable == null) continue;

            GameObject candidate = damageable.gameObject;
            if (!uniqueTargets.Add(candidate)) continue;
            if (candidate == lastHitTarget) continue;

            float dist = Vector2.Distance(transform.position, candidate.transform.position);
            if (dist < bestDist)
            {
                bestDist = dist;
                best = candidate.transform;
            }
        }

        target = best;
    }

    /// 첫 히트 이후, 카메라 월드 범위에서 다음 타겟 탐색
    private void TryAcquireCameraTarget()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            target = null;
            return;
        }

        List<Transform> candidates = SkillUtils.FindEnemyInCamera(
            transform.position,   // 투사체 기준
            cam,
            enemyLayer
        );

        // 아직 안 맞은 대상 우선
        Transform next = PickNextTarget(candidates, true);
        if (next != null)
        {
            target = next;
            return;
        }

        // 전부 한 번씩 맞췄으면 순환
        if (candidates.Count > 0)
        {
            hitTargetSet.Clear();
            target = PickNextTarget(candidates, false);
            return;
        }

        // 카메라 안에 후보 없음 → 귀환
        target = null;
    }

    private Transform PickNextTarget(List<Transform> candidates, bool excludeHitSet)
    {
        float bestDist = float.MaxValue;
        Transform best = null;

        foreach (var candidate in candidates)
        {
            if (candidate == null) continue;
            if (candidate.gameObject == lastHitTarget) continue;
            if (excludeHitSet && hitTargetSet.Contains(candidate.gameObject)) continue;

            float dist = Vector2.Distance(transform.position, candidate.position);
            if (dist < bestDist)
            {
                bestDist = dist;
                best = candidate;
            }
        }

        return best;
    }

    /// 귀환 상태로 전환
    private void BeginReturn()
    {
        returning = true;
        target = null;
    }

    /// 타겟의 시각적 중심 좌표 계산
    private Vector2 GetTargetCenter(Transform target)
    {
        if (target == null) return transform.position;

        Collider2D col = target.GetComponentInChildren<Collider2D>();
        if (col != null)
            return col.bounds.center;

        return target.position;
    }

    /// 타겟에 히트했을 때의 처리 (체인/귀환 분기)
    protected override void HandleHit(GameObject targetObj)
    {
        skill.OnHit(attacker, targetObj);

        HitCount++;
        hasFirstHit = true;
        lastHitTarget = targetObj;

        if (targetObj != null)
            hitTargetSet.Add(targetObj);

        target = null;

        if (HitCount >= maxHitCount)
        {
            BeginReturn();
            return;
        }

        TryAcquireCameraTarget();

        if (target == null)
            BeginReturn();
    }

    /// 에디터에서 최초 탐색 반경 시각화
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, initialSearchRadius);
    }
}
