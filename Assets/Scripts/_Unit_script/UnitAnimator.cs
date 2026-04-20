using System.Collections;
using UnityEngine;

public enum SkillAnimState
{
    None = 0,
    Reset = 1,
    Delay = 2,
    Execute = 3,
    Hit = 4,
    Tick = 5,
    PostDelay = 6
}

public abstract class UnitAnimator : MonoBehaviour
{
    [SerializeField] private Animator mainAnimator;

    protected Animator animator;
    protected AnimatorOverrideController overrideController;

    private Coroutine skillAnimCoroutine;
    private int skillAnimToken = 0;

    // 추가: Animator 파라미터
    private static readonly int SkillAnimStateHash = Animator.StringToHash("skillAnimState");

    public bool IsSkillAnimationPlaying { get; private set; }

    // 추가: 다음 훅 예약용
    private bool hasPendingHook = false;
    private SkillInstance pendingSkillInstance;
    private SkillHookType pendingHookType;

    protected virtual void Awake()
    {
        animator = mainAnimator != null ? mainAnimator : GetComponentInChildren<Animator>();
        RefreshAnimatorOverride();

        // 추가: 시작 기본값
        if (animator != null)
            animator.SetInteger(SkillAnimStateHash, (int)SkillAnimState.None);
    }

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

    public void TryPlaySkillAnimation(SkillInstance skillInstance, SkillHookType hookType)
    {
        AnimationClip clip = GetSkillHookClip(skillInstance, hookType);
        if (clip == null || animator == null)
            return;

        // 추가: 이미 재생 중이면 즉시 덮지 않고 예약 후 Reset 흐름으로 보냄
        if (IsSkillAnimationPlaying)
        {
            pendingSkillInstance = skillInstance;
            pendingHookType = hookType;
            hasPendingHook = true;

            RequestResetFromInterrupt();
            return;
        }

        StartSkillAnimation(skillInstance, hookType, clip);
    }

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

    // 추가: 실제 훅 시작 공용 진입점
    private void StartSkillAnimation(SkillInstance skillInstance, SkillHookType hookType, AnimationClip clip)
    {
        SkillAnimState animState = ConvertHookToAnimState(hookType);
        if (animState == SkillAnimState.None)
            return;

        if (overrideController == null)
            RefreshAnimatorOverride();

        if (overrideController == null)
            return;

        string placeholderName = GetPlaceholderClipName(hookType);
        if (string.IsNullOrEmpty(placeholderName))
            return;

        // 수정: 새 훅 시작 전에 이전 훅 종료 훅 정리
        OnSkillAnimationStarted(skillInstance, hookType);

        overrideController[placeholderName] = clip;

        skillAnimToken++;
        int currentToken = skillAnimToken;

        IsSkillAnimationPlaying = true;
        animator.SetInteger(SkillAnimStateHash, (int)animState);

        if (skillAnimCoroutine != null)
            StopCoroutine(skillAnimCoroutine);

        skillAnimCoroutine = StartCoroutine(PlayHookThenResolveRoutine(clip.length, currentToken));
    }

    // 수정: 자연 종료도 무조건 Reset으로 합류
    private IEnumerator PlayHookThenResolveRoutine(float clipLength, int token)
    {
        float hookWaitTime = Mathf.Max(clipLength, 0.01f);
        yield return new WaitForSeconds(hookWaitTime);

        if (token != skillAnimToken || animator == null)
            yield break;

        yield return RunResetAndResolveRoutine(token);
    }

    // 추가: 강제 종료 시 호출. None으로 바로 안 보내고 Reset으로 합류
    private void RequestResetFromInterrupt()
    {
        if (!IsSkillAnimationPlaying || animator == null)
            return;

        skillAnimToken++;
        int currentToken = skillAnimToken;

        if (skillAnimCoroutine != null)
        {
            StopCoroutine(skillAnimCoroutine);
            skillAnimCoroutine = null;
        }

        // 추가: 이전 훅 종료 처리 먼저
        OnSkillAnimationEnded();

        skillAnimCoroutine = StartCoroutine(RunResetAndResolveRoutine(currentToken));
    }

    // 추가: 모든 종료 경로를 여기로 통일
    private IEnumerator RunResetAndResolveRoutine(int token)
    {
        if (animator == null)
            yield break;

        animator.SetInteger(SkillAnimStateHash, (int)SkillAnimState.Reset);

        // 추가: Reset이 실제 샘플링되도록 최소 1프레임 보장
        yield return null;

        if (token != skillAnimToken || animator == null)
            yield break;

        // 추가: 필요하면 한 프레임 더 보장
        yield return null;

        if (token != skillAnimToken || animator == null)
            yield break;

        // 추가: 예약된 훅이 있으면 Reset 뒤에 바로 실행
        if (hasPendingHook)
        {
            SkillInstance nextSkillInstance = pendingSkillInstance;
            SkillHookType nextHookType = pendingHookType;

            hasPendingHook = false;
            pendingSkillInstance = null;

            AnimationClip nextClip = GetSkillHookClip(nextSkillInstance, nextHookType);

            if (nextClip != null)
            {
                StartSkillAnimation(nextSkillInstance, nextHookType, nextClip);
                yield break;
            }
        }

        // 수정: 예약 훅이 없을 때만 None으로 종료
        animator.SetInteger(SkillAnimStateHash, (int)SkillAnimState.None);

        skillAnimCoroutine = null;
        IsSkillAnimationPlaying = false;

        OnSkillAnimationEnded();
        RestoreBaseAnimation();
    }

    protected virtual SkillAnimState ConvertHookToAnimState(SkillHookType hookType)
    {
        return hookType switch
        {
            SkillHookType.Delay => SkillAnimState.Delay,
            SkillHookType.Execute => SkillAnimState.Execute,
            SkillHookType.Hit => SkillAnimState.Hit,
            SkillHookType.Tick => SkillAnimState.Tick,
            SkillHookType.PostDelay => SkillAnimState.PostDelay,
            _ => SkillAnimState.None
        };
    }

    protected virtual void OnSkillAnimationStarted(SkillInstance skillInstance, SkillHookType hookType) {}
    protected virtual void OnSkillAnimationEnded() {}

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

    protected abstract void RestoreBaseAnimation();
}
