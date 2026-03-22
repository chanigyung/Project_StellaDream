using UnityEngine;

[CreateAssetMenu(menuName = "SkillModule/Offense/Knockback")]
public class KnockbackModuleData : SkillModuleData
{
    public float knockbackX = 5f;
    public float knockbackY = 2f;

    public override ISkillModule CreateModule()
    {
        return new KnockbackModule(this);
    }
}
