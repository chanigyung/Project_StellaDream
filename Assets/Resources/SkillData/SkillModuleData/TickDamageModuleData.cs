using UnityEngine;

[CreateAssetMenu(menuName = "SkillModule/Offense/TickDamage")]
public class TickDamageModuleData : SkillModuleData
{
    public float damage = 20f;

    public override ISkillModule CreateModule()
    {
        return new TickDamageModule(this);
    }
}