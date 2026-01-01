using UnityEngine;

public class KnockbackModule : SkillModuleBase
{
    private readonly KnockbackModuleData data;

    public KnockbackModule(SkillInstance owner, KnockbackModuleData data)
        : base(owner)
    {
        this.data = data;
    }

    public override void OnHit(GameObject attacker, GameObject target)
    {
        SkillUtils.ApplyKnockback(
            attacker,
            target,
            data.knockbackX,
            data.knockbackY
        );
    }
}
