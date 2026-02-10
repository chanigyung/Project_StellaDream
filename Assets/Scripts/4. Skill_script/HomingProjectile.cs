using System.Collections.Generic;
using UnityEngine;

public class HomingProjectile : Projectile
{
    [Header("Targeting")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float initialStraightTime = 0.3f;
    [SerializeField] private float initialSearchRadius = 2.5f;

    [Header("Hit")]
    [SerializeField] private float hitRadius = 0.3f;

    [Header("Chain")]
    [SerializeField] private int maxHitCount = 5;

    [Header("Return")]
    [SerializeField] private float returnStopDistance = 0.4f;

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

    /// <summary>
    /// 투사체 초기화 (Homing은 lifetime을 사용하지 않음)
    /// </summary>
    public override void Initialize(GameObject attacker, SkillInstance skill, Vector2 direction, float speed, float lifetime)
    {
        base.Initialize(attacker, skill, direction, speed, 0f);

        caster = attacker != null ? attacker.transform : null;

        elapsed = 0f;
        hasFirstHit = false;
        returning = false;
        lastHitTarget = null;
        target = null;

        initialRetargetTimer = 0f;
        cameraRetargetTimer = 0f;
    }

    /// <summary>
    /// 투사체 전체 이동 루프 (직진 → 추적 → 귀환)
    /// </summary>
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

    /// <summary>
    /// 플레이어에게 귀환하는 이동 처리
    /// </summary>
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

    /// <summary>
    /// 최초 직진 이후, 근거리 범위에서 다음 타겟 탐색
    /// </summary>
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

    /// <summary>
    /// 첫 히트 이후, 카메라 월드 범위에서 다음 타겟 탐색
    /// </summary>
    private void TryAcquireCameraTarget()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            BeginReturn();
            return;
        }

        GameObject found = SkillUtils.FindEnemyInCamera(transform.position, cam, enemyLayer, lastHitTarget);
        target = found != null ? found.transform : null;
    }

    /// <summary>
    /// 귀환 상태로 전환
    /// </summary>
    private void BeginReturn()
    {
        returning = true;
        target = null;
    }

    /// <summary>
    /// 타겟의 시각적 중심 좌표 계산
    /// </summary>
    private Vector2 GetTargetCenter(Transform target)
    {
        if (target == null) return transform.position;

        Collider2D col = target.GetComponentInChildren<Collider2D>();
        if (col != null)
            return col.bounds.center;

        return target.position;
    }

    /// <summary>
    /// 타겟에 히트했을 때의 처리 (체인/귀환 분기)
    /// </summary>
    protected override void HandleHit(GameObject targetObj)
    {
        skill.OnHit(attacker, targetObj);

        HitCount++;
        hasFirstHit = true;
        lastHitTarget = targetObj;
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

    /// <summary>
    /// 에디터에서 최초 탐색 반경 시각화
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, initialSearchRadius);
    }
}
