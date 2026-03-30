using System.Collections;
using UnityEngine;

public abstract class UnitAnimator : MonoBehaviour
{
    protected Animator animator;
    protected AnimatorOverrideController overrideController;

    private Coroutine skillAnimCoroutine;
    private int skillAnimToken = 0;
    private int attackLayerIndex = -1;

    public bool IsSkillAnimationPlaying { get; private set; }

    protected virtual void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        RefreshAnimatorOverride();

        if (animator != null)
            attackLayerIndex = animator.GetLayerIndex("Attack Layer");

        if (animator != null && attackLayerIndex >= 0)
            animator.SetLayerWeight(attackLayerIndex, 0f);
    }

    // 몬스터처럼 런타임에 AnimatorController를 갈아끼우는 경우 다시 호출
    public void RefreshAnimatorOverride()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (animator == null || animator.runtimeAnimatorController == null)
            return;

        RuntimeAnimatorController baseController = animator.runtimeAnimatorController;

        if (baseController is AnimatorOverrideController existingOverride &&
            existingOverride.runtimeAnimatorController != null)
        {
            baseController = existingOverride.runtimeAnimatorController;
        }

        overrideController = new AnimatorOverrideController(baseController);
        animator.runtimeAnimatorController = overrideController;
    }

    // SkillInstance에서 공용으로 호출할 진입점
    public void TryPlaySkillAnimation(SkillInstance skillInstance, SkillHookType hookType)
    {
        AnimationClip clip = GetSkillHookClip(skillInstance, hookType);
        if (clip == null)
            return;

        PlaySkillHookAnimation(hookType, clip);
    }

    // Hook -> SkillData clip 선택
    protected virtual AnimationClip GetSkillHookClip(SkillInstance skillInstance, SkillHookType hookType)
    {
        SkillHookAnimationSet skillAnimations = skillInstance?.data?.skillAnimations;
        if (skillAnimations == null)
            return null;

        return hookType switch
        {
            SkillHookType.Delay => skillAnimations.delayClip,
            SkillHookType.Execute => skillAnimations.executeClip,
            SkillHookType.Hit => skillAnimations.hitClip,
            SkillHookType.Tick => skillAnimations.tickClip,
            SkillHookType.PostDelay => skillAnimations.postDelayClip,
            _ => null
        };
    }

    // 새 훅 애니메이션이 오면 즉시 덮어쓰기
    protected virtual void PlaySkillHookAnimation(SkillHookType hookType, AnimationClip clip)
    {
        if (clip == null || animator == null)
            return;

        if (overrideController == null)
            RefreshAnimatorOverride();

        if (overrideController == null)
            return;

        string placeholderName = GetPlaceholderClipName(hookType);
        string stateName = GetSkillStateName(hookType);

        if (string.IsNullOrEmpty(placeholderName) || string.IsNullOrEmpty(stateName))
            return;

        overrideController[placeholderName] = clip;

        skillAnimToken++;
        int currentToken = skillAnimToken;

        IsSkillAnimationPlaying = true;

        int layerIndex = attackLayerIndex >= 0 ? attackLayerIndex : 0;

        if (attackLayerIndex >= 0)
            animator.SetLayerWeight(attackLayerIndex, 1f);

        animator.Play(stateName, layerIndex, 0f);

        if (skillAnimCoroutine != null)
            StopCoroutine(skillAnimCoroutine);

        skillAnimCoroutine = StartCoroutine(EndSkillAnimationRoutine(clip.length, currentToken));
    }

    private IEnumerator EndSkillAnimationRoutine(float clipLength, int token)
    {
        float waitTime = Mathf.Max(clipLength, 0.01f);
        yield return new WaitForSeconds(waitTime);

        if (token != skillAnimToken)
            yield break;

        IsSkillAnimationPlaying = false;

        if (animator != null && attackLayerIndex >= 0)
            animator.SetLayerWeight(attackLayerIndex, 0f);

        skillAnimCoroutine = null;
        RestoreBaseAnimation();
    }

    protected virtual string GetPlaceholderClipName(SkillHookType hookType)
    {
        return hookType switch
        {
            SkillHookType.Delay => "Skill_Delay_Clip",
            SkillHookType.Execute => "Skill_Execute_Clip",
            SkillHookType.Hit => "Skill_Hit_Clip",
            SkillHookType.Tick => "Skill_Tick_Clip",
            SkillHookType.PostDelay => "Skill_PostDelay_Clip",
            _ => null
        };
    }

    protected virtual string GetSkillStateName(SkillHookType hookType)
    {
        return hookType switch
        {
            SkillHookType.Delay => "Skill_Delay",
            SkillHookType.Execute => "Skill_Execute",
            SkillHookType.Hit => "Skill_Hit",
            SkillHookType.Tick => "Skill_Tick",
            SkillHookType.PostDelay => "Skill_PostDelay",
            _ => null
        };
    }

    // 스킬 애니메이션 종료 후 각 유닛의 기본 상태 복귀
    protected abstract void RestoreBaseAnimation();
}