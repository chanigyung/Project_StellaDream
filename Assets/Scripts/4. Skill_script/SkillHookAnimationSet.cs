using UnityEngine;

[System.Serializable]
public class SkillHookAnimationSet
{
    public AnimationClip delayClip;
    public bool delayLockArmControl = true;

    public AnimationClip executeClip;
    public bool executeLockArmControl = true;

    public AnimationClip hitClip;
    public bool hitLockArmControl = true;

    public AnimationClip tickClip;
    public bool tickLockArmControl = true;

    public AnimationClip postDelayClip;
    public bool postDelayLockArmControl = true;
}