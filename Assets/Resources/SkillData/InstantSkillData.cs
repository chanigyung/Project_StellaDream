using UnityEngine;

[CreateAssetMenu(menuName = "Skill/Instant SkillData")]
public class InstantSkillData : SkillData
{
    public override SkillUseType UseType => SkillUseType.Instant;

    public override SkillInstance CreateInstance()
    {
        return new InstantSkillInstance(this);
    }
}