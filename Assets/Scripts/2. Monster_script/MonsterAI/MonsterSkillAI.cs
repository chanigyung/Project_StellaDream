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
        Debug.Log($"[SkillAI] skillInstances={context.instance.skillInstances.Count}, skillList={context.instance.data.skillList.Count}");
    }

    // 변경: AttackAction이 "지금 당장 시전 시작이 가능한지"만 빠르게 확인할 수 있도록 제공
    public bool CanCastNow()
    {
        if (context == null || context.instance == null) return false;
        if (context.target == null) return false;
        if (!context.isTracing || context.isCastingSkill) return false;

        if (IsBlocked()) return false;
        if (Time.time < stunOrKnockbackRecoverTime) return false;

        float dist = context.distanceToTarget;
        return SelectCastSkill(dist, out _, out _);
    }

    // 변경: AttackAction에서 호출. 실제로 시전을 "시작"하면 true 반환
    public bool TryUseSkill()
    {
        if (context == null || context.instance == null) return false;
        if (context.target == null) return false;
        if (!context.isTracing || context.isCastingSkill) return false;

        if (IsBlocked()) return false;
        if (Time.time < stunOrKnockbackRecoverTime) return false;

        float dist = context.distanceToTarget;
        if (!SelectCastSkill(dist, out SkillInstance skill, out float maxRange))
            return false;

        // 좌우 방향 계산(현재는 X축 기반. 추후 2D 전방/에임 방식으로 확장 가능)
        float xDiff = context.target.transform.position.x - transform.position.x;
        Vector2 dir = new Vector2(Mathf.Sign(xDiff), 0f);

        StartCoroutine(CastSkillWithDelay(skill, dir));
        return true;
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

    private IEnumerator CastSkillWithDelay(SkillInstance skill, Vector2 dir)
    {
        context.isCastingSkill = true;
        context.UpdateContext();
        //정지하고 공격 애니메이션
        context.movement?.Stop();
        context.animator?.PlayAttack();

        //딜레이 대기후에
        yield return new WaitForSeconds(skill.delay);

        //스킬 실행
        bool success = skillExecutor.UseSkill(skill, dir);

        //실행됐다면 쿨타임 적용
        if (success)
        {
            lastUsedTimes[skill] = Time.time;
            lastGlobalSkillUseTime = Time.time;
        }
        //스킬 후딜레이
        context.movement?.Stop();
        yield return new WaitForSeconds(skill.postDelay);
        //캐스팅 상태 종료
        context.isCastingSkill = false;

        context.UpdateContext();
    }

    public void NotifyRecoverDelay()
    {
        stunOrKnockbackRecoverTime = Time.time + recoverBlockDuration;
    }
}