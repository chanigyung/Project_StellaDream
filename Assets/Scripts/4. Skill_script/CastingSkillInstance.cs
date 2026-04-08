public class CastingSkillInstance : SkillInstance
{
    private readonly CastingSkillData castingData;

    public override SkillUseType UseType => SkillUseType.Casting;

    public float CastTime => castingData != null ? castingData.castTime : 0f;
    
    public CastingSkillInstance(CastingSkillData data) : base(data)
    {
        castingData = data;
    }
}