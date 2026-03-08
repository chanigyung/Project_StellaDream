using UnityEngine;

public class DamageModule : SkillModuleBase
{
    private readonly float damage;

    public DamageModule(SkillInstance owner, float damage) : base(owner)
    {
        this.damage = damage;
    }

    public override void OnHit(SkillContext context)
    {
        SkillUtils.ApplyDamage(context.targetObject, damage);
    }
}
