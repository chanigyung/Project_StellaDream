using UnityEngine;

public class MonsterContext : UnitContext
{
    // === 몬스터 전용 참조 ===
    public MonsterMovement movement;
    public MonsterAnimator animator;
    public MonsterTraceHandler traceHandler;
    public MonsterSkillAI skillAI;

    public MonsterInstance monsterInstance => instance as MonsterInstance;
    public bool isFlyingMonster = false;
    public Vector2 flyingAnchorPosition;
    public Vector2 flyingWanderTarget;
    public bool hasFlyingWanderTarget = false;

    // === 추적 상태 판단용 변수 ===
    public bool isTracing = false;                 
    public bool isTracePermanent = false;          
    public bool isTraceReleasedPending = false;    
    public float traceReleaseTimer = 0f;           

    // 상태이상일때 몬스터 추적용 플래그
    public bool attackMonster = false;

    public override void UpdateContext()
    {
        base.UpdateContext();

        // 몬스터는 넉백 중 이동/행동 차단 유지
        canMove = !isKnockbacked && !isCastingSkill && !isMoveSkillActive;
        canAct = !isKnockbacked && !isMoveSkillActive;
        // 몬스터는 추적 중일 때만 공격 가능
        canAttack = isTracing && canAct && !isCastingSkill;
    }
}
