using UnityEngine;

[CreateAssetMenu(menuName = "SkillModule/Object/AreaHitbox")]
public class AreaHitboxModuleData : HitboxModuleData
{
    public float tickInterval = 0.5f;
    public hitEffect tickHitEffect = new hitEffect();
    
    public override ISkillModule CreateModule()
    {
        return new AreaHitboxModule(this);
    }
}
