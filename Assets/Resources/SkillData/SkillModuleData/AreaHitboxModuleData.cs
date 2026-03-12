using UnityEngine;

[CreateAssetMenu(menuName = "SkillModule/AreaHitbox")]
public class AreaHitboxModuleData : HitboxModuleData
{
    public float tickInterval = 0.5f;
    
    public override ISkillModule CreateModule()
    {
        return new AreaHitboxModule(this);
    }
}