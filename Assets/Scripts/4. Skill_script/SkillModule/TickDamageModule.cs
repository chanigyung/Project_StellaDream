using UnityEngine;

public class TickDamageModule : SkillModuleBase
{
    private readonly TickDamageModuleData data;
    private readonly float damage;

    public TickDamageModule(TickDamageModuleData data)
    {
        this.data = data;
        this.damage = data.damage;
    }

    public override void OnTick(SkillContext context)
    {
        if (context.targetObject == null) return;

        SkillUtils.ApplyDamage(context.targetObject, damage);
    }
}