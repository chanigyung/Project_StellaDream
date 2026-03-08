using UnityEngine;

public class KnockbackModule : SkillModuleBase
{
    private readonly KnockbackModuleData data;

    public KnockbackModule(SkillInstance owner, KnockbackModuleData data)
        : base(owner)
    {
        this.data = data;
    }

    public override void OnHit(SkillContext context)
    {
        SkillUtils.ApplyKnockback(context.attacker, context.targetObject, data.knockbackX, data.knockbackY);
    }
}
