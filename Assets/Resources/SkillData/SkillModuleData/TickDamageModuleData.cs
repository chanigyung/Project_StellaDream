using UnityEngine;

[CreateAssetMenu(menuName = "Skill/Module/TickDamage")]
public class TickDamageModuleData : SkillModuleData
{
    public float damage = 20f;

    public override ISkillModule CreateModule()
    {
        return new TickDamageModule(this);
    }
}