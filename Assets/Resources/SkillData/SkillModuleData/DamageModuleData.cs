using UnityEngine;

[CreateAssetMenu(menuName = "SkillModule/Damage")]
public class DamageModuleData : SkillModuleData
{
    public float damage = 5f;

    public override ISkillModule CreateModule(SkillInstance owner)
    {
        return new DamageModule(owner, damage);
    }
}
