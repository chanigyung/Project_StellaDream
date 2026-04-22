using UnityEngine;

[CreateAssetMenu(menuName = "SkillModule/Object/AreaHitbox")]
public class AreaHitboxModuleData : HitboxModuleData
{
    public float tickInterval = 0.5f;
    public hitEffect tickHitEffect = new hitEffect();

    protected override void OnEnable()
    {
        EnsureTags(SkillTag.Area, SkillTag.Hitbox, SkillTag.Damage, SkillTag.Install);
    }

    protected override void OnValidate()
    {
        EnsureTags(SkillTag.Area, SkillTag.Hitbox, SkillTag.Damage, SkillTag.Install);
    }
    
    public override ISkillModule CreateModule()
    {
        return new AreaHitboxModule(this);
    }
}
