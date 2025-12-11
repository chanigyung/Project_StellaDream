using UnityEngine;

public class ProjectileSkillInstance : SkillInstance, IProjectileInfo
{
    public float damage;
    public float lifetime;
    public float speed;

    public float Damage => damage;
    public float Speed => speed;
    public float Lifetime => lifetime;
    public GameObject ProjectilePrefab => projectilePrefab;

    public GameObject projectilePrefab;

    public ProjectileSkillInstance(ProjectileSkillData data) : base(data)
    {
        damage = data.baseDamage;
        lifetime = data.projectileLifetime;
        speed = data.projectileSpeed;
        projectilePrefab = data.projectilePrefab;

        if (data.statusEffects != null)
        {
            foreach (var effect in data.statusEffects)
            {
                statusEffects.Add(CopyEffect(effect));
            }
        }
    }

    public override void Execute(GameObject attacker, Vector2 direction)
    {
        SkillUtils.SpawnProjectile(attacker, this, direction);
    }

    public override void OnHit(GameObject attacker, GameObject target)
    {
        SkillUtils.ApplyDamage(target, damage);

        // SkillUtils.ApplyKnockback(attacker, target, SkillUtils.GetKnockbackDirection(this, attacker, target));
    }

    public override void ApplyUpgrade(WeaponUpgradeInfo upgrade)
    {
        if (upgrade == null) return;

        var data = (ProjectileSkillData)baseData;

        damage = damage + data.damagePerUpgrade * upgrade.baseUpgradeLevel;
        lifetime = lifetime + data.lifetimePerUpgrade * upgrade.efficiencyUpgradeLevel;
        speed = speed + data.projSpeedPerUpgrade * upgrade.masteryUpgradeLevel;
        
        ApplyStatusEffectUpgrade(upgrade.masteryUpgradeLevel);
    }
}
