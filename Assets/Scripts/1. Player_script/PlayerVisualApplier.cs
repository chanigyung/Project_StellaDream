using UnityEngine;

public class PlayerVisualApplier : MonoBehaviour
{
    private const string FaceClipName = "Face_Clip";
    private const string BodyClipName = "Body_Clip";
    private const string TailClipName = "Tail_Clip";
    private const string BackEffectClipName = "BackEffect_Clip";

    [Header("Head")]
    public SpriteRenderer head;
    public Animator face;
    public SpriteRenderer hair0;
    public SpriteRenderer hair1;
    public SpriteRenderer hair2;

    [Header("Animated Body Parts")]
    public Animator body;
    public Animator tail;
    public Animator backEffect;

    [Header("Arms & Hands")]
    public SpriteRenderer leftArm;
    public SpriteRenderer rightArm;
    public SpriteRenderer leftHand;
    public SpriteRenderer rightHand;

    [Header("Legs & Feet")]
    public SpriteRenderer leftLeg;
    public SpriteRenderer rightLeg;
    public SpriteRenderer leftFoot;
    public SpriteRenderer rightFoot;

    private AnimatorOverrideController faceOverrideController;
    private AnimatorOverrideController bodyOverrideController;
    private AnimatorOverrideController tailOverrideController;
    private AnimatorOverrideController backEffectOverrideController;

    public void ApplyVisual(PlayerVisualData visual)
    {
        head.sprite = visual.head;
        hair0.sprite = visual.hair0;
        hair1.sprite = visual.hair1;
        hair2.sprite = visual.hair2;

        ApplyClip(face, ref faceOverrideController, FaceClipName, visual.faceClip);
        ApplyClip(body, ref bodyOverrideController, BodyClipName, visual.bodyClip);
        ApplyClip(tail, ref tailOverrideController, TailClipName, visual.tailClip);
        ApplyClip(backEffect, ref backEffectOverrideController, BackEffectClipName, visual.backEffectClip);

        leftArm.sprite = visual.leftArm;
        rightArm.sprite = visual.rightArm;
        leftHand.sprite = visual.leftHand;
        rightHand.sprite = visual.rightHand;

        leftLeg.sprite = visual.leftLeg;
        rightLeg.sprite = visual.rightLeg;
        leftFoot.sprite = visual.leftFoot;
        rightFoot.sprite = visual.rightFoot;
    }

    private void ApplyClip(Animator animator, ref AnimatorOverrideController overrideController, string clipName, AnimationClip clip)
    {
        if (animator == null || animator.runtimeAnimatorController == null || clip == null)
            return;

        if (overrideController == null)
        {
            RuntimeAnimatorController baseController = animator.runtimeAnimatorController;
            if (baseController is AnimatorOverrideController existingOverride &&
                existingOverride.runtimeAnimatorController != null)
            {
                baseController = existingOverride.runtimeAnimatorController;
            }

            overrideController = new AnimatorOverrideController(baseController);
            animator.runtimeAnimatorController = overrideController;
        }

        overrideController[clipName] = clip;
        animator.Rebind();
        animator.Update(0f);
    }
}
