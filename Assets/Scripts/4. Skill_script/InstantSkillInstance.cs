public class InstantSkillInstance : SkillInstance
{
    public override SkillUseType UseType => SkillUseType.Instant;

    public InstantSkillInstance(SkillData data) : base(data)
    {
    }
}