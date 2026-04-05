using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class SkillExecutor : MonoBehaviour
{
    private Dictionary<SkillInstance, float> lastUsedTimeDict = new();

    private SkillInstance activeSkill = null; //사용중인 스킬
    private readonly HashSet<SkillInstance> heldSkill = new();

    // 외부에서 현재 락 여부 확인시
    public bool IsCastLocked => activeSkill != null;

    public bool UseSkill(SkillContext context)
    {
        return TryExecuteSkill(context, true);
    }

    public bool TryExecuteSkill(SkillContext context, bool useInternalCooldown)
    {
        SkillInstance skillInstance = context.skillInstance;

        if (skillInstance == null) return false;
        if (skillInstance.IsLocked) return false;

        if (activeSkill != null &&
            activeSkill != skillInstance &&
            !skillInstance.data.ignoreCastLock)
        {
            return false;
        }

        if (useInternalCooldown)
        {
            if (lastUsedTimeDict.TryGetValue(skillInstance, out float lastUsed))
            {
                if (Time.time < lastUsed + skillInstance.cooldown)
                    return false;
            }

            lastUsedTimeDict[skillInstance] = Time.time;
        }

        if (!skillInstance.data.ignoreCastLock)
            activeSkill = skillInstance;

        StartCoroutine(ExecuteSkillDelay(context));
        return true;
    }

    private IEnumerator ExecuteSkillDelay(SkillContext context)
    {
        SkillInstance skill = context.skillInstance;
        if (skill.delay > 0f)
        {
            skill.Delay(context);
            yield return new WaitForSeconds(skill.delay);
        }

        skill.Execute(context);

        skill.PostDelay(context);

        if (skill.postDelay > 0f)
            yield return new WaitForSeconds(skill.postDelay);

        ReleaseActiveSkill(skill);
    }

    // 홀드형 스킬 시작시 호출
    public void BeginHeldSkill(SkillInstance skillInstance)
    {
        if (skillInstance == null) return;

        heldSkill.Add(skillInstance);

        if (!skillInstance.data.ignoreCastLock)
            activeSkill = skillInstance;
    }

    // 홀드형 스킬 종료시 호출
    public void EndHeldSkill(SkillInstance skillInstance)
    {
        if (skillInstance == null) return;
        if (!heldSkill.Contains(skillInstance)) return;

        heldSkill.Remove(skillInstance);
        ReleaseActiveSkill(skillInstance);
    }

    // activeSkill 해제 공통 함수
    private void ReleaseActiveSkill(SkillInstance skillInstance)
    {
        if (activeSkill == skillInstance)
            activeSkill = null;
    }

    public SkillContext CreateCastContext(
    SkillInstance skillInstance,
    GameObject attacker,
    GameObject targetObject,
    Vector3 position,
    Vector2 direction)
    {
        Vector2 normalizedDirection = direction.sqrMagnitude > 0.0001f
            ? direction.normalized
            : Vector2.right;

        float angle = Mathf.Atan2(normalizedDirection.y, normalizedDirection.x) * Mathf.Rad2Deg;

        SkillContext context = new SkillContext
        {
            skillInstance = skillInstance,
            attacker = attacker,
            contextOwner = attacker,
            sourceObject = null,
            targetObject = targetObject,
            position = position,
            rotation = Quaternion.Euler(0f, 0f, angle),
            direction = normalizedDirection,
            hasDirection = true,
            spawnPointType = skillInstance != null ? skillInstance.SpawnPointType : SkillSpawnPointType.Center
        };

        SkillUtils.FillContextSpawnPoints(ref context, attacker);
        return context;
    }

    public SkillContext CreateCastContext(
        SkillInstance skillInstance,
        GameObject attacker,
        Vector2 direction)
    {
        return CreateCastContext(
            skillInstance,
            attacker,
            null,
            attacker != null ? attacker.transform.position : Vector3.zero,
            direction
        );
    }

    public SkillContext CreateCastContext(
        SkillInstance skillInstance,
        GameObject attacker,
        GameObject targetObject,
        Vector2 direction)
    {
        return CreateCastContext(
            skillInstance,
            attacker,
            targetObject,
            attacker != null ? attacker.transform.position : Vector3.zero,
            direction
        );
    }
}
