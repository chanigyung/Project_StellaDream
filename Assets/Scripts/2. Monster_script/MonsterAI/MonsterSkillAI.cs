using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSkillAI : MonoBehaviour
{
    [SerializeField] private SkillExecutor skillExecutor;
    [SerializeField] private float globalSkillCooldown = 3f;

    private MonsterContext context;
    private Transform player;
    private float lastGlobalSkillUseTime;
    private StatusEffectManager eManager;

    private Dictionary<SkillInstance, float> lastUsedTimes = new();

    private float recoverBlockDuration = 0.5f; // 기절/넉백 회복 후 0.5초간 스킬 금지
    private float stunOrKnockbackRecoverTime = -999f; //초기값

    private void Start()
    {
        var controller = GetComponent<MonsterController>();
        context = controller.Context;
        eManager = GetComponent<StatusEffectManager>();

        player = GameObject.FindWithTag("Player")?.transform;

        // 초기화
        foreach (var skill in context.instance.skillInstances)
        {
            if (skill != null)
                lastUsedTimes[skill] = -999f; // 초기값
        }

        InvokeRepeating(nameof(TryUseSkill), 0f, 0.1f); // 반복 체크
    }

    private void TryUseSkill()
    {
        if (player == null || context == null || context.instance == null) return;
        if (!context.isTracing || context.isCastingSkill) return;

        if (eManager != null) //기절, 파워넉백일 경우 스킬사용X
        {
            foreach (var effect in eManager.GetActiveEffects())
            {
                if (effect.effectType == StatusEffectType.Stun ||
                    effect.effectType == StatusEffectType.PowerKnockback)
                {
                    return; // 차단
                }
            }
        }

        if (Time.time < stunOrKnockbackRecoverTime)
            return;

        float dist = Vector2.Distance(transform.position, player.position);

        for (int i = 0; i < context.instance.skillInstances.Count; i++)
        {
            SkillInstance skill = context.instance.skillInstances[i];
            float range = context.instance.data.skillList[i].maxRange;

            if (skill == null) continue;
            if (dist > range) continue; // 거리 조건 불충족
            if (Time.time < lastUsedTimes[skill] + skill.cooldown) continue; // 스킬 쿨타임
            if (Time.time < lastGlobalSkillUseTime + globalSkillCooldown) continue; // 공통 쿨타임

            // 좌우 방향 계산
            float xDiff = player.position.x - transform.position.x;
            Vector2 dir = new Vector2(Mathf.Sign(xDiff), 0f);

            StartCoroutine(CastSkillWithDelay(skill, dir));
            return;
        }
    }

    private IEnumerator CastSkillWithDelay(SkillInstance skill, Vector2 dir)
    {
        context.isCastingSkill = true;
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
        yield return new WaitForSeconds(skill.postDelay);
        //캐스팅 상태 종료
        context.isCastingSkill = false;
    }

    public void NotifyRecoverDelay()
    {
        stunOrKnockbackRecoverTime = Time.time + recoverBlockDuration;
    }
}