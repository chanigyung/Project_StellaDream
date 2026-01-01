using UnityEngine;

public class DamageModule : SkillModuleBase
{
    private readonly float damage;

    public DamageModule(SkillInstance owner, float damage) : base(owner)
    {
        this.damage = damage;
    }

    public override void OnHit(GameObject attacker, GameObject target)
    {
        SkillUtils.ApplyDamage(target, damage);
    }
}
