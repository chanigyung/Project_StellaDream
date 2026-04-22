using UnityEngine;

public enum ChainTriggerHook
{
    Hit,
    Expire
}

[CreateAssetMenu(menuName = "SkillModule/ChainSkill")]
public class ChainSkillModuleData : SkillModuleData
{
    [Header("연계 발동 조건")]
    public ChainTriggerHook triggerHook;

    [Header("연계로 실행할 스킬")]
    public SkillData reactionSkillData;

    private void OnEnable()
    {
        EnsureTags(SkillTag.Chain);
    }

    private void OnValidate()
    {
        EnsureTags(SkillTag.Chain);
    }

    public override ISkillModule CreateModule()
    {
        return new ChainSkillModule(this);
    }
}
