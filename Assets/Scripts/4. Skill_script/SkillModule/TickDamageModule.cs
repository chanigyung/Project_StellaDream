using UnityEngine;

public class TickDamageModule : SkillModuleBase
{
    private readonly TickDamageModuleData data;

    public TickDamageModule(TickDamageModuleData data)
    {
        this.data = data;
    }

    public override void OnTick(SkillContext context)
    {
        if (context.targetObject == null) return;

        SkillUtils.ApplyDamage(context.targetObject, data.damage);
    }
}