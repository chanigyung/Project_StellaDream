using System.Collections;
using UnityEngine;

public class SkillExecutor : MonoBehaviour
{
    private SkillInstance activeSkill = null;

    private Coroutine castingCoroutine;
    private CastingSkillInstance currentCastingSkill;

    public bool IsCasting => currentCastingSkill != null;
    public bool IsCastLocked => activeSkill != null;

    public bool UseSkill(SkillContext context)
    {
        context.EnsureValues();

        SkillInstance skillInstance = context.skillInstance;

        if (skillInstance == null)
            return false;

        if (context.coreInstance != null)
            context.coreInstance.ApplyValues(ref context);

        if (!skillInstance.CanUse())
            return false;

        if (activeSkill != null &&
            activeSkill != skillInstance &&
            !skillInstance.data.ignoreCastLock)
        {
            return false;
        }

        if (skillInstance is InstantSkillInstance instantSkillInstance)
        {
            skillInstance.StartCooldown(context.values.cooldownMultiplier);

            if (!skillInstance.data.ignoreCastLock)
                activeSkill = skillInstance;

            StartCoroutine(ExecuteInstantSkill(context, instantSkillInstance));
            return true;
        }

        if (skillInstance is CastingSkillInstance castingSkillInstance)
        {
            skillInstance.StartCooldown(context.values.cooldownMultiplier);

            if (!skillInstance.data.ignoreCastLock)
                activeSkill = skillInstance;

            BeginCastingSkill(context, castingSkillInstance);
            return true;
        }

        return false;
    }

    public bool IsCurrentCastingSkill(SkillInstance skillInstance)
    {
        return currentCastingSkill == skillInstance;
    }

    public void CancelCurrentCasting()
    {
        if (currentCastingSkill == null)
            return;

        if (castingCoroutine != null)
        {
            StopCoroutine(castingCoroutine);
            castingCoroutine = null;
        }

        CastingSkillInstance canceledSkill = currentCastingSkill;
        currentCastingSkill = null;

        canceledSkill.CancelCast();
        ReleaseActiveSkill(canceledSkill);
    }

    private IEnumerator ExecuteInstantSkill(SkillContext context, InstantSkillInstance skillInstance)
    {
        if (skillInstance.delay > 0f)
        {
            skillInstance.Delay(context);
            yield return new WaitForSeconds(skillInstance.delay);
        }

        skillInstance.Execute(context);
        skillInstance.PostDelay(context);

        if (skillInstance.postDelay > 0f)
            yield return new WaitForSeconds(skillInstance.postDelay);

        ReleaseActiveSkill(skillInstance);
    }

    private void BeginCastingSkill(SkillContext context, CastingSkillInstance skillInstance)
    {
        currentCastingSkill = skillInstance;

        if (castingCoroutine != null)
            StopCoroutine(castingCoroutine);

        castingCoroutine = StartCoroutine(ExecuteCastingSkill(context, skillInstance));
    }

    private IEnumerator ExecuteCastingSkill(SkillContext context, CastingSkillInstance skillInstance)
    {
        skillInstance.BeginCast(context);

        if (skillInstance.delay > 0f)
            yield return new WaitForSeconds(skillInstance.delay);

        skillInstance.StartCast(context);

        float tickTimer = 0f;
        float castDuration = skillInstance.MaxCastTime;
        float castTickInterval = skillInstance.CastTickInterval;
        float castElapsedTime = 0f;

        while (castDuration <= 0f || castElapsedTime < castDuration)
        {
            if (castTickInterval > 0f)
            {
                tickTimer += Time.deltaTime;

                while (tickTimer >= castTickInterval)
                {
                    skillInstance.TickCast(context);
                    tickTimer -= castTickInterval;
                }
            }

            if (castDuration > 0f)
                castElapsedTime += Time.deltaTime;

            yield return null;
        }

        skillInstance.EndCast(context);

        if (skillInstance.postDelay > 0f)
            yield return new WaitForSeconds(skillInstance.postDelay);

        skillInstance.ReleaseAllSpawnedObjects();

        castingCoroutine = null;
        currentCastingSkill = null;
        ReleaseActiveSkill(skillInstance);
    }

    private void ReleaseActiveSkill(SkillInstance skillInstance)
    {
        if (activeSkill == skillInstance)
            activeSkill = null;
    }
}
