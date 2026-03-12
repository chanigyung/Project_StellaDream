using UnityEngine;

public class KnockbackModule : SkillModuleBase
{
    private readonly KnockbackModuleData data;
    private readonly float knockbackX;
    private readonly float knockbackY;

    public KnockbackModule(KnockbackModuleData data)
    {
        this.data = data;
        this.knockbackX = data.knockbackX;
        this.knockbackY = data.knockbackY;
    }

    public override void OnHit(SkillContext context)
    {
        SkillUtils.ApplyKnockback(context.attacker, context.targetObject, knockbackX, knockbackY);
    }
}
