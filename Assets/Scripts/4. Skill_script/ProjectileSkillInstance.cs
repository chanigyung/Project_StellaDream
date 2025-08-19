using UnityEngine;

public class ProjectileSkillInstance : SkillInstance
{
    public float damage;
    public float lifetime;
    public float speed;

    public float distanceFromUser;
    public GameObject projectilePrefab;

    public RuntimeAnimatorController effectAnimator;
    public float effectDuration;

    public ProjectileSkillInstance(ProjectileSkillData data) : base(data)
    {
        damage = data.baseDamage;
        lifetime = data.projectileLifetime;
        speed = data.projectileSpeed;

        distanceFromUser = data.hitboxDistanceFromUser;
        projectilePrefab = data.projectilePrefab;

        effectAnimator = data.skillEffectAnimation;
        effectDuration = data.skillEffectDuration;

        if (data.statusEffects != null)
        {
            foreach (var effect in data.statusEffects)
            {
                statusEffects.Add(CopyEffect(effect));
            }
        }
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
