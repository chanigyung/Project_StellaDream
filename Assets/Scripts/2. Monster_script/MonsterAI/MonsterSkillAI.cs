using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSkillAI : MonoBehaviour
{
    [SerializeField] private SkillExecutor skillExecutor;
    [SerializeField] private float globalSkillCooldown = 3f;

    private MonsterContext context;
    private float lastGlobalSkillUseTime;

    private Dictionary<SkillInstance, float> lastUsedTimes = new();

    public void Initialize(MonsterContext ctx)
    {
        context = ctx;
    }

    public bool CanCastNow()
    {
        if (context == null || context.instance == null) return false;
        if (!CanStartCast()) return false;

        float dist = (context.target != null) ? context.distanceToTarget : 0f;
        return SelectCastSkill(dist, out _, out _);
    }

    public bool TryUseSkill()
    {
        if (context == null || context.instance == null) return false;
        if (!CanStartCast()) return false;

        float dist = (context.target != null) ? context.distanceToTarget : 0f;

        if (!SelectCastSkill(dist, out SkillInstance skill, out _))
            return false;

        if (skill is CastingSkillInstance castingSkill && castingSkill.MaxCastTime <= 0f)
        {
            Debug.LogWarning($"[MonsterSkillAI] Monster '{gameObject.name}' tried to use infinite casting skill '{castingSkill.data.name}'. Monsters do not support MaxCastTime <= 0.");
            return false;
        }

        Vector2 direction = GetCastDirection();
        SkillContext skillContext = SkillUtils.CreateSkillContext(
            skill,
            gameObject,
            direction,
            context.target,
            null,
            SkillInputSlot.None,
            SkillInputPhase.Pressed);

        StartCoroutine(CastSkillWithDelay(skill, skillContext));
        return true;
    }

    private bool CanStartCast()
    {
        if (!context.canAttack || context.isCastingSkill)
            return false;

        if (!context.isFlyingMonster && !context.isGrounded)
            return false;

        return true;
    }

    private Vector2 GetCastDirection()
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

    private bool SelectCastSkill(float dist, out SkillInstance pickedSkill, out float pickedRange)
    {
        pickedSkill = null;
        pickedRange = 0f;

        int instanceCount = context.monsterInstance.skillInstances.Count;
        int dataCount = context.monsterInstance.data.skillList != null ? context.monsterInstance.data.skillList.Count : 0;
        int count = Mathf.Min(instanceCount, dataCount);

        for (int i = 0; i < count; i++)
        {
            SkillInstance skill = context.monsterInstance.skillInstances[i];
            if (skill == null) continue;

            float range = context.monsterInstance.data.skillList[i].maxRange;
            if (dist > range) continue;
            if (!skill.CanUse()) continue;
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

        context.movement?.Stop();
        context.animator?.PlayAttack();

        bool success = skillExecutor.UseSkill(skillContext);

        if (success)
        {
            lastGlobalSkillUseTime = Time.time;

            float totalCastTime = GetTotalSkillDuration(skill);
            if (totalCastTime > 0f)
                yield return new WaitForSeconds(totalCastTime);
        }

        context.isCastingSkill = false;
        context.UpdateContext();
    }

    private float GetTotalSkillDuration(SkillInstance skill)
    {
        float totalDuration = Mathf.Max(0f, skill.delay) + Mathf.Max(0f, skill.postDelay);

        if (skill is CastingSkillInstance castingSkill)
            totalDuration += Mathf.Max(0f, castingSkill.MaxCastTime);

        return totalDuration;
    }
}
