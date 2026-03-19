using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSkillAI : MonoBehaviour
{
    [SerializeField] private SkillExecutor skillExecutor;
    [SerializeField] private float globalSkillCooldown = 3f;

    private MonsterContext context;
    private float lastGlobalSkillUseTime;
    private StatusEffectManager eManager;

    private Dictionary<SkillInstance, float> lastUsedTimes = new();

    private float recoverBlockDuration = 0.5f; // 기절/넉백 회복 후 0.5초간 스킬 금지
    private float stunOrKnockbackRecoverTime = -999f; //초기값

    // 변경: MonsterController에서 context를 주입받는 초기화 함수(AttackAction에서 호출 가능하도록 준비)
    public void Initialize(MonsterContext ctx)
    {
        context = ctx;
        eManager = GetComponent<StatusEffectManager>();

        // 초기화: 인스턴스의 스킬별 마지막 사용 시간 기록
        lastUsedTimes.Clear();
        if (context?.instance?.skillInstances != null)
        {
            foreach (var skill in context.instance.skillInstances)
            {
                if (skill != null)
                    lastUsedTimes[skill] = -999f;
            }
        }
    }

    // 변경: AttackAction이 "지금 당장 시전 시작이 가능한지"만 빠르게 확인할 수 있도록 제공
    public bool CanCastNow()
    {
        if (context == null || context.instance == null) return false;
        if (!context.canAttack || context.isCastingSkill) return false;

        if (IsBlocked()) return false;
        if (Time.time < stunOrKnockbackRecoverTime) return false;

        float dist = (context.target != null) ? context.distanceToTarget : 0f;
        return SelectCastSkill(dist, out _, out _);
    }

    // 변경: AttackAction에서 호출. 실제로 시전을 "시작"하면 true 반환
    public bool TryUseSkill()
    {
        if (context == null || context.instance == null) return false;
        if (!context.canAttack || context.isCastingSkill) return false;

        if (IsBlocked()) return false;
        if (Time.time < stunOrKnockbackRecoverTime) return false;

        float dist = (context.target != null) ? context.distanceToTarget : 0f;

        if (!SelectCastSkill(dist, out SkillInstance skill, out float maxRange))
            return false;

        Vector2 direction = GetCastDirection();
        SkillContext skillContext = skillExecutor.CreateCastContext(skill, gameObject, context.target, direction);

        StartCoroutine(CastSkillWithDelay(skill, skillContext));
        return true;
    }

    private Vector2 GetCastDirection()//스킬 사용방향 계산
    {
        if (context.target != null)
        {
            float xDiff = context.target.transform.position.x - transform.position.x;
            return new Vector2(Mathf.Sign(xDiff), 0f);
        }

        float fx = context.facingDirectionX;
        if (Mathf.Approximately(fx, 0f)) fx = 1f;

        return new Vector2(Mathf.Sign(fx), 0f);
    }

    // 변경: 기절/파워넉백 상태면 스킬 사용 차단 (기존 로직 유지)
    private bool IsBlocked()
    {
        if (eManager == null) return false;

        foreach (var effect in eManager.GetActiveEffects())
        {
            if (effect.effectType == StatusEffectType.Stun ||
                effect.effectType == StatusEffectType.PowerKnockback)
            {
                return true;
            }
        }

        return false;
    }

    // 변경: 거리/쿨타임/공용쿨타임 조건으로 "지금 시전 가능한 스킬"을 선택
    private bool SelectCastSkill(float dist, out SkillInstance pickedSkill, out float pickedRange)
    {
        pickedSkill = null;
        pickedRange = 0f;

        // skillList와 skillInstances는 인덱스가 대응된다는 전제(기존 구조 유지)
        // 변경: 데이터 리스트 길이 불일치 방어
        int instanceCount = context.instance.skillInstances.Count;
        int dataCount = context.instance.data.skillList != null ? context.instance.data.skillList.Count : 0;
        int count = Mathf.Min(instanceCount, dataCount);

        for (int i = 0; i < count; i++)
        {
            SkillInstance skill = context.instance.skillInstances[i];
            if (skill == null) continue;

            float range = context.instance.data.skillList[i].maxRange;
            if (dist > range) continue;
            if (Time.time < lastUsedTimes[skill] + skill.cooldown) continue;
            if (Time.time < lastGlobalSkillUseTime + globalSkillCooldown) continue;

            pickedSkill = skill;
            pickedRange = range;
            return true;
        }

        return false;
    }

    private IEnumerator CastSkillWithDelay(SkillInstance skill, SkillContext skillContext)
    {
        context.isCastingSkill = true;
        context.UpdateContext();

        // 정지하고 공격 애니메이션
        context.movement?.Stop();
        context.animator?.PlayAttack();

        // 변경: 몬스터 쪽에서 delay/postDelay를 직접 기다리지 않는다.
        //       실제 딜레이 처리는 SkillExecutor(UseSkill 내부)에서만 담당한다.
        //       여기서는 "몬스터가 캐스팅 중" 상태를 유지하기 위한 시간만 기다린다.
        bool success = skillExecutor.UseSkill(skillContext);

        if (success)
        {
            // 변경: 스킬이 '시작'되었다면 즉시 쿨타임을 기록(중복 시전 방지)
            lastUsedTimes[skill] = Time.time;
            lastGlobalSkillUseTime = Time.time;

            float totalCastTime = Mathf.Max(0f, skill.delay) + Mathf.Max(0f, skill.postDelay);
            if (totalCastTime > 0f)
                yield return new WaitForSeconds(totalCastTime);
        }

        // 캐스팅 상태 종료
        context.isCastingSkill = false;
        context.UpdateContext();
    }

    public void NotifyRecoverDelay()
    {
        stunOrKnockbackRecoverTime = Time.time + recoverBlockDuration;
    }
}