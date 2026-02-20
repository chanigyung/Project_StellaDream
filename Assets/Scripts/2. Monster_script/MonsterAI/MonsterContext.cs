using UnityEngine;

public class MonsterContext
{
    // === 캐싱 컴포넌트 참조 ===
    public Transform selfTransform;
    public GameObject target;
    public MonsterMovement movement;
    public MonsterAnimator animator;
    public MonsterInstance instance;

    // === 상태이상 관련 변수 ===
    public bool isStunned;
    public bool isRooted;
    public bool isKnockbacked;

    // === 추적 상태 판단용 변수 ===
    public bool isTracing = false;                 // 현재 추적 중인지
    public bool isTracePermanent = false;          // 공격으로 인해 영구추적 상태 여부
    public bool isTraceReleasedPending = false;    // 추적 해제 대기시간중인지?
    public float traceReleaseTimer = 0f;           // 추적 해제까지 남은 시간
    public bool isCastingSkill = false;

    // 상태이상일때 몬스터 추적용 플래그
    public bool attackMonster = false;
    public MonsterSkillAI skillAI;

    // === 업데이트 계산 변수 ===
    public Vector2 directionToTarget { get; private set; }
    public float distanceToTarget { get; private set; }
    public float targetDirectionX { get; private set; }
    public bool canMove { get; private set; }
    public bool canAct { get; private set; }

    public void UpdateContext()
    {
        if (target != null)
        {
            Vector2 delta = target.transform.position - selfTransform.position;
            directionToTarget = delta.normalized;
            distanceToTarget = delta.magnitude;
            targetDirectionX = Mathf.Sign(delta.x);
        }
        else
        {
            directionToTarget = Vector2.zero;
            distanceToTarget = float.MaxValue;
            targetDirectionX = 1f;
        }

        canMove = !isStunned && !isRooted && !isKnockbacked && !isCastingSkill;
        canAct  = !isStunned && !isKnockbacked;
    }
}
