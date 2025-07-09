using System.Collections.Generic;
using UnityEngine;

public class MonsterSkillAI : MonoBehaviour
{
    [SerializeField] private SkillExecutor skillExecutor;
    [SerializeField] private float globalSkillCooldown = 3f;

    private MonsterInstance monster;
    private Transform player;
    private float lastGlobalSkillUseTime;
    private StatusEffectManager eManager;

    private Dictionary<SkillInstance, float> lastUsedTimes = new();

    private float recoverBlockDuration = 0.5f; // 기절/넉백 회복 후 0.5초간 스킬 금지
    private float stunOrKnockbackRecoverTime = -999f; //초기값

    private void Start()
    {
        var controller = GetComponent<MonsterController>();
        monster = controller.instance as MonsterInstance;
        eManager = GetComponent<StatusEffectManager>();

        player = GameObject.FindWithTag("Player")?.transform;

        // 초기화
        foreach (var skill in monster.skillInstances)
        {
            if (skill != null)
                lastUsedTimes[skill] = -999f; // 초기값
        }

        InvokeRepeating(nameof(TryUseSkill), 0f, 0.1f); // 반복 체크
    }

    private void TryUseSkill()
    {
        if (player == null || monster == null) return;
        if (!GetComponent<MonsterMovement>()?.isTracing ?? true) return;

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

        for (int i = 0; i < monster.skillInstances.Count; i++)
        {
            SkillInstance skill = monster.skillInstances[i];
            float range = monster.data.skillList[i].maxRange;

            if (skill == null) continue;
            if (dist > range) continue; // 거리 조건 불충족
            if (Time.time < lastUsedTimes[skill] + skill.cooldown) continue; // 스킬 쿨타임
            if (Time.time < lastGlobalSkillUseTime + globalSkillCooldown) continue; // 공통 쿨타임

            // 스킬 실행
            Vector2 dir = (player.position - transform.position).normalized;
            bool success = skillExecutor.UseSkill(skill, dir);

            if (success)
            {
                lastUsedTimes[skill] = Time.time;
                lastGlobalSkillUseTime = Time.time;
            }

            return; // 하나만 사용
        }

        // 사용 가능한 스킬 없음 → 추적 유지
        GetComponent<MonsterMovement>()?.SetTracing(true);
    }

    public void NotifyRecoverDelay()
    {
        stunOrKnockbackRecoverTime = Time.time + recoverBlockDuration;
    }
}