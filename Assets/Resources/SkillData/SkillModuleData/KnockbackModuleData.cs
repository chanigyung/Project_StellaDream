using UnityEngine;

[CreateAssetMenu(menuName = "SkillModule/Knockback")]
public class KnockbackModuleData : SkillModuleData
{
    public float knockbackX = 5f;
    public float knockbackY = 2f;

    public override ISkillModule CreateModule(SkillInstance owner)
    {
        return new KnockbackModule(owner, this);
    }
}
